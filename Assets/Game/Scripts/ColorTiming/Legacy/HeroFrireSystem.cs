using System;
using ColorTiming.Input;
using ColorTiming.Input.Adapters;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class HeroFrireSystem : MonoBehaviour, IGameInputConsumer, IGameplayPointerConsumer, ITransientEntityConsumer
{
    public GameObject mao_nor;
    public GameObject mao_jiandao;
    public GameObject mao_chuizi;
    public GameObject mao_zhadan;
    public GameObject mao_dao;
    public GameObject mao_futou;
    public GameObject mao_feiji;

    public GameObject sk_nor;
    public GameObject sk_jiandao;
    public GameObject sk_jiandao2;
    public GameObject sk_chuizi;
    public GameObject sk_zhadan;
    public GameObject sk_dao;
    public GameObject sk_futou;
    public GameObject sk_feiji;

    IGameInput gameInput;
    IGameplayPointerWorld pointerWorld;
    ITransientEntityService transientEntities;

    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    public void BindGameplayPointer(IGameplayPointerWorld pointer)
    {
        pointerWorld = pointer ?? throw new ArgumentNullException(nameof(pointer));
    }

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    public void OnFire(Weapon weapon,float look,string parm)
    {
        int filp = look > 0 ? 1 : -1;
        Vector3 pos = Vector3.zero;
        GameObject _sk = sk_nor;
        Transform append = null;
        switch (weapon.weaponType)
        {
            case WeaponType.nor: 
                pos = mao_nor.transform.localPosition;
                _sk = sk_nor;
                break;
            case WeaponType.jiandao:
                pos = mao_jiandao.transform.localPosition;
                if (parm == "2")
                {
                    _sk = sk_jiandao2;
                }
                else
                {
                    _sk = sk_jiandao;
                }
                break;
            case WeaponType.chuizhi:
                pos = mao_chuizi.transform.localPosition;
                _sk = sk_chuizi;
                break;
            case WeaponType.zhadan:
                pos = mao_zhadan.transform.localPosition;
                _sk = sk_zhadan;
                Vector3 world = ResolvePointerWorld();
                
                parm = ((int)weapon.colorType + 1).ToString();
                parm = parm + "=" + world.ToString();
                //射线位置
                //Vector3 normal = (world - transform.position).normalized;
                break;
            case WeaponType.meigongdao:
                append = this.transform;
                pos = mao_dao.transform.localPosition;
                _sk = sk_dao;
                    break;

            case WeaponType.futou:
                pos = mao_futou.transform.localPosition;
                _sk = sk_futou;
                    break;
            case WeaponType.zhifeiji:
                pos = mao_feiji.transform.localPosition;
                _sk = sk_feiji;
                Vector3 world_f = ResolvePointerWorld();

                parm = ((int)weapon.colorType + 1).ToString();
                parm = parm + "=" + world_f.ToString();
                break;
            default:
                break;
        }

        pos = new Vector3(pos.x * filp,pos.y,pos.z);
        pos += transform.position;

        //print("kankan创建位置" + pos);
        CreateSkill(_sk,weapon,pos,filp,parm,append);
    }

    Vector3 ResolvePointerWorld()
    {
        if (gameInput == null || pointerWorld == null)
        {
            throw new InvalidOperationException("Hero fire input has not been bound by the scene composition root.");
        }

        Vector2 world = pointerWorld.Resolve(gameInput.PointerScreenPosition);
        return new Vector3(world.x, world.y, 0f);
    }

    
    void CreateSkill(GameObject sk_,Weapon weapon,Vector3 pos,int _filp,string parm,Transform append)
    {
        if (sk_ == null)
        {
            throw new InvalidOperationException("The requested skill prefab is not assigned.");
        }
        if (transientEntities == null)
        {
            throw new InvalidOperationException("Transient entities have not been bound by the composition root.");
        }

        transientEntities.Spawn(
            sk_.name,
            pos,
            Quaternion.identity,
            append,
            instance => instance.GetComponent<Skill_base>()?.SetSkillData(
                gameObject,
                weapon,
                _filp,
                parm));
    }


}
