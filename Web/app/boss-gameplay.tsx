'use client';

import { useEffect, useRef } from 'react';

export function BossGameplay() {
  const video = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const element = video.current;
    if (!element) return;
    const observer = new IntersectionObserver(([entry]) => {
      if (!entry.isIntersecting) element.pause();
    });
    const pauseInBackground = () => {
      if (document.hidden) element.pause();
    };
    observer.observe(element);
    document.addEventListener('visibilitychange', pauseInBackground);
    return () => {
      observer.disconnect();
      document.removeEventListener('visibilitychange', pauseInBackground);
      element.pause();
    };
  }, []);

  return (
    <video
      ref={video}
      className="boss-gameplay"
      src="/media/boss2-gameplay-upright.mp4"
      width={1280}
      height={720}
      controls
      playsInline
      preload="metadata"
      aria-label="狂躁的书虫 Boss2 实机战斗，28 秒无声视频"
    >
      <track
        kind="captions"
        src="/media/boss2-gameplay.zh.vtt"
        srcLang="zh"
        label="画面说明"
      />
      你的浏览器不支持视频播放。
      <a href="/media/boss2-gameplay-upright.mp4" download>
        下载 Boss2 实机视频
      </a>
    </video>
  );
}
