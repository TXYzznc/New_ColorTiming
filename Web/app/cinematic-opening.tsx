'use client';

/* oxlint-disable jsx-a11y/media-has-caption -- Decorative game-menu animation; the scene is described by aria-label and adjacent hero text. */

import { useEffect, useRef, useState } from 'react';

export function CinematicOpening() {
  const opening = useRef<HTMLVideoElement>(null);
  const loop = useRef<HTMLVideoElement>(null);
  // Include the curtain in prerendered HTML, before the client bundle loads.
  const [intro, setIntro] = useState(true);
  const [introReady, setIntroReady] = useState(false);
  const [started, setStarted] = useState(false);
  const [looping, setLooping] = useState(false);
  const [loopReady, setLoopReady] = useState(false);
  const [muted, setMuted] = useState(true);
  const [paused, setPaused] = useState(false);
  const [visible, setVisible] = useState(true);
  const [blocked, setBlocked] = useState(false);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)');
    let timer: ReturnType<typeof setTimeout>;
    function configure() {
      clearTimeout(timer);
      setIntroReady(true);
      setIntro(!reduced.matches && !location.hash);
      setPaused(reduced.matches);
      if (reduced.matches || location.hash) setStarted(true);
      else
        timer = setTimeout(() => {
          setIntro(false);
          setStarted(true);
        }, 3100);
    }
    configure();
    reduced.addEventListener('change', configure);
    return () => {
      clearTimeout(timer);
      reduced.removeEventListener('change', configure);
    };
  }, []);

  useEffect(() => {
    let inView = true;
    const update = () => setVisible(inView && !document.hidden);
    const observer = new IntersectionObserver(([entry]) => {
      inView = entry.isIntersecting;
      update();
    });
    if (opening.current) observer.observe(opening.current);
    document.addEventListener('visibilitychange', update);
    return () => {
      observer.disconnect();
      document.removeEventListener('visibilitychange', update);
    };
  }, []);

  useEffect(() => {
    const current = looping ? loop.current : opening.current;
    const other = looping ? opening.current : loop.current;
    other?.pause();
    if (!current) return;
    if (!started || paused || !visible || failed) {
      current.pause();
      return;
    }
    let cancelled = false;
    current
      .play()
      .then(() => {
        if (!cancelled) setBlocked(false);
      })
      .catch(() => {
        if (!cancelled) setBlocked(true);
      });
    return () => {
      cancelled = true;
      current.pause();
    };
  }, [started, paused, visible, looping, failed]);

  function replay() {
    setFailed(false);
    setLooping(false);
    setLoopReady(false);
    setPaused(false);
    setStarted(true);
    setIntro(false);
    if (opening.current) {
      opening.current.currentTime = 0;
      opening.current.play().catch(() => setBlocked(true));
    }
  }
  function togglePlayback() {
    if (blocked) {
      (looping ? loop.current : opening.current)
        ?.play()
        .then(() => setBlocked(false))
        .catch(() => setBlocked(true));
      setPaused(false);
    } else setPaused(!paused);
  }
  return (
    <>
      <div className="hero-art cinematic-art">
        <video
          ref={opening}
          className="cinematic-video"
          src="/media/opening.mp4"
          muted={muted}
          playsInline
          preload="auto"
          aria-label="失物语游戏开场动画"
          onEnded={() => setLooping(true)}
          onError={() => setFailed(true)}
        />
        {started && (
          <video
            ref={loop}
            className={`cinematic-video cinematic-loop ${looping && loopReady && !failed ? 'is-playing' : ''}`}
            src="/media/idle.mp4"
            muted={muted}
            playsInline
            loop
            preload="auto"
            aria-label="失物语主菜单循环动画"
            onPlaying={() => setLoopReady(true)}
            onError={() => setFailed(true)}
          />
        )}
      </div>
      <div className="cinematic-controls">
        <span className="cinematic-caption">
          {failed ? '视频暂不可用' : '原作开场 / THE LOST TALES'}
        </span>
        <button type="button" onClick={togglePlayback} disabled={failed}>
          {paused || blocked ? '播放动画' : '暂停动画'}
        </button>
        <button
          type="button"
          onClick={() => setMuted(!muted)}
          aria-pressed={!muted}
        >
          {muted ? '开启声音' : '关闭声音'}
        </button>
        <button type="button" onClick={replay}>
          重播开场
        </button>
      </div>
      {intro && (
        <div className={`opening-curtain${introReady ? ' is-ready' : ''}`}>
          <div className="opening-panel opening-panel-top" />
          <div className="opening-panel opening-panel-bottom" />
          <div className="opening-name" aria-label="失物语 The Lost Tales">
            <div className="opening-chinese" aria-hidden="true">
              失物语
            </div>
            <div className="opening-english" aria-hidden="true">
              THE LOST TALES
            </div>
          </div>
          <span className="opening-index">一场关于被遗忘之物的冒险</span>
        </div>
      )}
      <noscript>
        <style>{'.opening-curtain { display: none !important; }'}</style>
      </noscript>
    </>
  );
}

export function ScrollMotion() {
  useEffect(() => {
    const media = window.matchMedia('(prefers-reduced-motion: reduce)');
    if (media.matches) return;
    const elements = document.querySelectorAll(
      '.section-heading, .chapter-grid, .character-feature, .comic-strip figure, .rule-heading, .weapon-tabs, .weapon-detail, .boss-title, .boss-entry, .art-grid > *, .cta-section h2',
    );
    const observer = new IntersectionObserver(
      (entries) =>
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('motion-enter');
            observer.unobserve(entry.target);
          }
        }),
      { threshold: 0.08 },
    );
    elements.forEach((element) => observer.observe(element));
    return () => observer.disconnect();
  }, []);
  return null;
}
