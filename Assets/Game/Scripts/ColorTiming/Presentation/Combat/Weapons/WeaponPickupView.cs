
using System;
using ColorTiming.Combat;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class WeaponPickupView : MonoBehaviour, IColorTimingSoundConsumer, IFrameworkEntityParticipant
{
    public Sprite[] sprites;
    public Sprite[] sprites_outline;

    public GameObject tip1;
    public GameObject tip2;

    public WeaponIdentity Weapon { get; private set; }
    public bool HasWeapon { get; private set; }
    PlayerActorView hero;
    public AudioClip entAudio;
    IColorTimingSoundService soundService;
    Action frameworkRelease;

    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    public void OnFrameworkEntitySpawned() { }

    public void OnFrameworkEntityDespawned()
    {
        UnsubscribeHero();
        HasWeapon = false;
        frameworkRelease = null;
    }

    bool pickingUp;
    BoxCollider2D boxCollider;
    SpriteRenderer spr;

    bool isEnter;

    float fadeProgress;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
        spr = GetComponent<SpriteRenderer>();
        spr.color = new Color(0f, 0f, 0f, 0f);
    }

    private void OnEnable()
    {
        fadeProgress = 0f;
        pickingUp = false;
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (spr == null || boxCollider == null)
        {
            return;
        }

        if(fadeProgress < 1)
        {
            fadeProgress = Mathf.Min(1f, fadeProgress + Time.deltaTime);
            spr.color = new Color(fadeProgress, fadeProgress, fadeProgress, fadeProgress);
        }
        else if(!boxCollider.enabled)
        {
            boxCollider.enabled = true;
            spr.color = Color.white;
        }
    }

    public void InitPickWeapon(WeaponIdentity weapon)
    {
        Weapon = weapon;
        HasWeapon = true;

        SetSprite(false);
    }

    void SetSprite(bool outline)
    {
        if (!spr) spr = GetComponent<SpriteRenderer>();
        int ityp = Weapon.ToLegacyAnimatorIndex() - 1;
        if (ityp > -1 && ityp < sprites.Length)
        {
            Sprite s = null;
            if (outline)
            {
                s = sprites_outline[ityp];
            }
            else
            {
                s = sprites[ityp];
            }
            if (spr && s) { spr.sprite = s; }
        }
    }

    //当玩家有武器的时候拾取 ，没有武器的时候不执行
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enteringHero = collision.GetComponent<PlayerActorView>();
        if (enteringHero == null)
        {
            return;
        }

        UnsubscribeHero();
        hero = enteringHero;
        hero.OnPickUPWeapon.AddListener(TryPickup);
        SetInWeapon(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (hero == null || collision.GetComponent<PlayerActorView>() != hero)
        {
            return;
        }

        SetInWeapon(false);
        UnsubscribeHero();
    }

    private void OnDisable()
    {
        UnsubscribeHero();
    }

    void SetInWeapon(bool enter)
    {
        if (hero != null && !pickingUp)
        {
            //float line = enter ? 5 : 0;
            //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            //spr.GetPropertyBlock(mpb);
            //mpb.SetFloat("_lineWidth", line);
            //spr.SetPropertyBlock(mpb);
            SetSprite(enter);
            isEnter = enter;

            if (enter)
            {
                soundService?.Play(entAudio, ColorTimingSoundChannel.Player, transform.position);
            }
        }

    }

    public void ShowTip(int y)
    {
        if(y > 1)
        {
            tip2?.SetActive(true);
        }
        else
        {
            tip1?.SetActive(true);
        }
    }

    public void HideTip()
    {
        tip2?.SetActive(false);
        tip1?.SetActive(false);
    }

    void TryPickup()
    {
        if (!isEnter) return;

        if (hero != null && HasWeapon && !pickingUp)
        {
            if (hero.PickUPWeapon(Weapon))
            {
                pickingUp = true;
                UnsubscribeHero();
                if (frameworkRelease != null) frameworkRelease();
                else Destroy(gameObject);
            }
        }
    }

    private void UnsubscribeHero()
    {
        if (hero != null)
        {
            hero.OnPickUPWeapon.RemoveListener(TryPickup);
            hero = null;
        }
        isEnter = false;
    }
}
