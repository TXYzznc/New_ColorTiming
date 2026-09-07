import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: '失物语｜ColorTiming',
  description: '一个关于颜色、选择与失物的 Boss 战游戏。',
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="zh-CN"><body>{children}</body>
    </html>
  );
}
