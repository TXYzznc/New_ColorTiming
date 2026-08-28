#!/usr/bin/env python3
"""校验 Game Scripts 的 README、中文职责头、关键方法注释及代码不变性。"""

from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

from document_game_scripts import HEADER_MARKER, SCRIPT_ROOT, find_method_candidates


PROJECT_ROOT = Path(__file__).resolve().parents[1]
CHINESE = re.compile(r"[\u4e00-\u9fff]")


def strip_csharp_comments(text: str) -> str:
    output: list[str] = []
    index = 0
    state = "code"
    while index < len(text):
        char = text[index]
        next_char = text[index + 1] if index + 1 < len(text) else ""
        if state == "code":
            if char == '"':
                state = "string"
                output.append(char)
            elif char == "'":
                state = "char"
                output.append(char)
            elif char == "/" and next_char == "/":
                state = "line_comment"
                index += 1
            elif char == "/" and next_char == "*":
                state = "block_comment"
                index += 1
            else:
                output.append(char)
        elif state == "string":
            output.append(char)
            if char == "\\" and next_char:
                output.append(next_char)
                index += 1
            elif char == '"':
                state = "code"
        elif state == "char":
            output.append(char)
            if char == "\\" and next_char:
                output.append(next_char)
                index += 1
            elif char == "'":
                state = "code"
        elif state == "line_comment":
            if char in "\r\n":
                output.append(char)
                state = "code"
        elif state == "block_comment":
            if char == "*" and next_char == "/":
                state = "code"
                index += 1
            elif char in "\r\n":
                output.append(char)
        index += 1
    return "".join(output)


def normalized_code(text: str) -> str:
    text = text.lstrip("\ufeff")
    without_comments = strip_csharp_comments(text.replace("\r\n", "\n").replace("\r", "\n"))
    return re.sub(r"\s+", "", without_comments)


def head_text(relative_path: str) -> str | None:
    result = subprocess.run(
        ["git", "show", f"HEAD:{relative_path}"],
        cwd=PROJECT_ROOT,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return result.stdout if result.returncode == 0 else None


def has_chinese_comment_before(lines: list[str], index: int) -> bool:
    return any(line.strip().startswith("//") and CHINESE.search(line) for line in lines[max(0, index - 4):index])


def main() -> int:
    issues: list[str] = []
    directories = [SCRIPT_ROOT, *sorted(path for path in SCRIPT_ROOT.rglob("*") if path.is_dir())]
    scripts = sorted(SCRIPT_ROOT.rglob("*.cs"))
    method_count = 0

    for directory in directories:
        readme = directory / "README.md"
        if not readme.is_file():
            issues.append(f"缺少 README: {directory.relative_to(PROJECT_ROOT).as_posix()}")
        elif not CHINESE.search(readme.read_text(encoding="utf-8-sig")):
            issues.append(f"README 缺少中文说明: {readme.relative_to(PROJECT_ROOT).as_posix()}")

    for script in scripts:
        relative = script.relative_to(PROJECT_ROOT).as_posix()
        current = script.read_text(encoding="utf-8-sig")
        if not current.startswith(HEADER_MARKER) or not CHINESE.search(current.splitlines()[0]):
            issues.append(f"缺少中文职责头: {relative}")
        lines = current.replace("\r\n", "\n").replace("\r", "\n").split("\n")
        candidates = find_method_candidates(lines)
        method_count += len(candidates)
        for index, name, _, _ in candidates:
            if not has_chinese_comment_before(lines, index):
                issues.append(f"关键方法缺少中文注释: {relative}:{index + 1} {name}")
        baseline = head_text(relative)
        if baseline is None:
            issues.append(f"HEAD 中不存在脚本，无法验证代码不变性: {relative}")
        elif normalized_code(current) != normalized_code(baseline):
            issues.append(f"去除注释后代码发生变化: {relative}")

    report = {
        "passed": not issues,
        "directoryCount": len(directories),
        "scriptCount": len(scripts),
        "keyMethodCount": method_count,
        "issues": issues,
    }
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if not issues else 1


if __name__ == "__main__":
    sys.exit(main())
