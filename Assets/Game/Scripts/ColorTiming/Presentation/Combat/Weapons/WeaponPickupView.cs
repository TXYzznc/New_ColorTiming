// 文件职责：负责 武器拾取 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Combat / Weapons。

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

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
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

    // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
        spr = GetComponent<SpriteRenderer>();
        spr.color = new Color(0f, 0f, 0f, 0f);
    }

    // 组件启用时注册监听并同步当前状态。
    private void OnEnable()
    {
        fadeProgress = 0f;
        pickingUp = false;
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
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

    // 执行InitPick武器对应的主要流程。
    public void InitPickWeapon(WeaponIdentity weapon)
    {
        Weapon = weapon;
        HasWeapon = true;

        SetSprite(false);
    }

    // 设置Sprite，并使后续流程使用最新状态。
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
        hero.PreloadWeaponAnimation(Weapon);
        hero.OnPickUPWeapon.AddListener(TryPickup);
        SetInWeapon(true);
    }

    // 响应TriggerExit2D回调，并更新本对象状态。
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (hero == null || collision.GetComponent<PlayerActorView>() != hero)
        {
            return;
        }

        SetInWeapon(false);
        UnsubscribeHero();
    }

    // 组件停用时解除监听并停止临时流程。
    private void OnDisable()
    {
        UnsubscribeHero();
    }

    // 设置In武器，并使后续流程使用最新状态。
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

    // 显示Tip并同步当前数据。
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

    // 隐藏Tip并停止相关交互。
    public void HideTip()
    {
        tip2?.SetActive(false);
        tip1?.SetActive(false);
    }

    // 尝试拾取，并通过返回值报告是否成功。
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
                // 执行Destroy对应的主要流程。
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
