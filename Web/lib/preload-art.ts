// Share in-flight requests between the gallery warm-up and an actual selection.
const images = new Map<string, Promise<HTMLImageElement>>();

export function preloadArt(src: string): Promise<HTMLImageElement> {
  const cached = images.get(src);
  if (cached) return cached;
  const pending = new Promise<HTMLImageElement>((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = () => reject(new Error(`Image unavailable: ${src}`));
    image.src = src;
  });
  images.set(src, pending);
  void pending.catch(() => images.delete(src));
  return pending;
}

// Keep background transfers bounded so they do not monopolize video bandwidth.
export function warmArt(sources: string[]) {
  let cancelled = false;
  const queue = [...new Set(sources)];
  async function worker() {
    while (!cancelled && queue.length) {
      const src = queue.shift()!;
      await preloadArt(src).catch(() => undefined);
    }
  }
  void worker();
  void worker();
  return () => { cancelled = true; };
}
