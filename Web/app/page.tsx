'use client';
import { useEffect, useRef, useState } from 'react';

const chapters = [{ id: 'world', label: '01 / 失物之地' }, { id: 'rule', label: '02 / 颜色法则' }, { id: 'boss', label: '03 / 首领战' }];

export default function Home() {
  const [active, setActive] = useState('world');
  const [weapon, setWeapon] = useState('紫');
  const [intro, setIntro] = useState(true);
  const shellRef = useRef<HTMLElement>(null);
  useEffect(() => {
    const timer = window.setTimeout(() => setIntro(false), 2400);
    const sections = [...document.querySelectorAll<HTMLElement>('[data-chapter]')];
    const observer = new IntersectionObserver((entries) => entries.forEach((entry) => entry.isIntersecting && setActive(entry.target.id)), { rootMargin: '-35% 0px -55% 0px' });
    sections.forEach((section) => observer.observe(section));
    const onScroll = () => shellRef.current?.style.setProperty('--scroll', `${window.scrollY}`);
    const onMove = (event: MouseEvent) => { const x = (event.clientX / window.innerWidth - .5) * 2; const y = (event.clientY / window.innerHeight - .5) * 2; shellRef.current?.style.setProperty('--mx', `${x}`); shellRef.current?.style.setProperty('--my', `${y}`); };
    window.addEventListener('scroll', onScroll, { passive: true }); window.addEventListener('mousemove', onMove, { passive: true });
    return () => { window.clearTimeout(timer); observer.disconnect(); window.removeEventListener('scroll', onScroll); window.removeEventListener('mousemove', onMove); };
  }, []);
  return <main ref={shellRef} className="site-shell">
    {intro && <div className="intro-loader" aria-label="正在进入失物之地"><div className="loader-logo">COLOR<br/><span>TIMING</span></div><div className="loader-sigil">✦</div><div className="loader-line"><span /></div><p>ENTER THE LOST WORLD</p></div>}
    <header className="topbar"><a className="brand" href="#top"><span className="brand-mark">◈</span><span>COLOR<br/>TIMING</span></a><nav className="nav-tabs" aria-label="页面导航">{chapters.map(c => <a key={c.id} className={active===c.id?'active':''} href={'#'+c.id} onClick={()=>setActive(c.id)}>{c.label.split(' / ')[1].toUpperCase()}</a>)}</nav><div className="top-actions"><a className="wish-link" href="#cta">加入愿望单 ↗</a><span className="page-count">01—05</span></div></header>
    <section id="top" className="hero panel-dark"><div className="hero-copy"><p className="eyebrow">A COLOR-DRIVEN BOSS BATTLE</p><h1>失物语<br/><em>THE LOST TALES</em></h1><p className="hero-desc">在失物之地，颜色决定命运。拾起正确的武器，击穿首领的弱点。</p><div className="hero-actions"><a className="button button-light" href="#world">开始阅读 ↓</a><a className="text-link" href="#cta">加入愿望单 ↗</a></div></div><div className="hero-art"><img src="/images/cover.png" alt="失物语主视觉"/></div><div className="hero-scribble">◒</div></section>
    <section id="world" data-chapter className="chapter panel-paper"><div className="chapter-tag">01 / LOST WORLD</div><div className="chapter-grid"><div className="chapter-copy"><p className="eyebrow ink">THE WORLD</p><h2>所有被遗忘的东西，<br/><span>都在这里醒来。</span></h2><p>你从一场没有终点的梦中醒来，走进由失物堆叠而成的世界。每一片阴影都藏着故事，每一个首领都守着一件不该被忘记的东西。</p><a className="text-link ink-link" href="#rule">继续深入 ↓</a></div><div className="comic-frame"><img src="/images/boss2-scene.jpg" alt="失物之地的战斗场景"/><span className="caption">THE PLACE WHERE LOST THINGS GATHER</span></div></div></section>
    <section id="rule" data-chapter className="rule-section panel-purple"><div className="chapter-tag">02 / COLOR RULE</div><div className="rule-layout"><div><p className="eyebrow">THE CORE MECHANIC</p><h2>看清颜色。<br/><i>选择武器。</i></h2><p className="rule-lead">首领的弱点会改变。对应颜色的攻击才能真正造成伤害。</p></div><div className="color-wheel" role="group" aria-label="选择武器颜色">{['红','绿','紫','橙'].map(item=><button key={item} className={`color-dot color-${item} ${weapon===item?'selected':''}`} onClick={()=>setWeapon(item)} aria-pressed={weapon===item}>{item}</button>)}</div><div className="rule-card"><span className="rule-number">0{['红','绿','紫','橙'].indexOf(weapon)+1}</span><h3>{weapon}色武器已就绪</h3><p>观察首领身上的颜色，拾取对应武器，在下一次攻击窗口到来前完成选择。</p><div className="mini-meter"><span className={`meter-fill meter-${weapon}`}/></div></div></div></section>
    <section id="boss" data-chapter className="boss-section panel-ink"><div className="chapter-tag">03 / BOSS ENCOUNTERS</div><div className="boss-title"><p className="eyebrow">THE THINGS THAT REMAIN</p><h2>每一次相遇，<br/><span>都是一场选择。</span></h2></div><div className="boss-stage"><img src="/images/boss2-fight.jpg" alt="首领战斗画面"/><div className="stage-note">MOVE · READ · STRIKE</div></div><div className="boss-footer"><p>躲开攻击，重新定位，抓住颜色变化的瞬间。两场连续首领战，等待你走到最后。</p><a className="button button-outline" href="#cta">查看完整玩法</a></div></section>
    <section id="cta" className="cta-section panel-red"><div className="cta-inner"><p className="eyebrow">YOUR TURN STARTS HERE</p><h2>准备好找回<br/><em>失去的东西了吗？</em></h2><div className="cta-actions"><a className="button button-light large" href="#">加入愿望单 ↗</a><a className="button button-ghost large" href="#">下载试玩 Demo ↓</a></div><p className="cta-note">ColorTiming · PC Demo 即将开放</p></div><div className="cta-glyph">✦</div></section>
    <footer className="footer"><div className="brand"><span className="brand-mark">◈</span><span>COLOR<br/>TIMING</span></div><p>一个关于颜色、选择与失物的 Boss 战游戏。</p><span>© 2026 COLOR TIMING</span></footer>
  </main>;
}
