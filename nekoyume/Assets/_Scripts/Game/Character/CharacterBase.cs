using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BTAI;
using Nekoyume.Data.Table;
using Nekoyume.Game.CC;
using Nekoyume.Game.Controller;
using Nekoyume.Game.VFX;
using Nekoyume.UI;
using UnityEngine;

namespace Nekoyume.Game.Character
{
    public abstract class CharacterBase : MonoBehaviour
    {
        protected const float AnimatorTimeScale = 1.2f;
        
        public Root Root;
        public int HP = 0;
        public int ATK = 0;
        public int DEF = 0;

        public int Power = 100;

        public virtual WeightType WeightType { get; protected set; } = WeightType.Small;
        public float RunSpeed = 0.0f;

        public int HPMax { get; protected set; } = 0;
        protected ProgressBar _hpBar = null;
        protected ProgressBar _castingBar = null;
        protected virtual Vector3 _hudOffset => new Vector3();
        protected float _dyingTime = 1.0f;

        protected const float kSkillGlobalCooltime = 0.6f;

        public bool Rooted => gameObject.GetComponent<IRoot>() != null;
        public bool Silenced => gameObject.GetComponent<ISilence>() != null;
        public bool Stunned => gameObject.GetComponent<IStun>() != null;
        protected virtual float Range { get; set; }
        public string targetTag = "";
        public bool attackEnd { get; private set; }
        public bool hitEnd { get; private set; }
        public bool dieEnd { get; private set; }
        public abstract float Speed { get; }

        public ICharacterAnimator animator { get; protected set; }
        public abstract Guid Id { get; }

        protected virtual void Awake()
        {
            Event.OnAttackEnd.AddListener(AttackEnd);
        }

        protected virtual void OnDisable()
        {
            RunSpeed = 0.0f;
            Root = null;
            DisableHUD();
        }

        public bool IsDead()
        {
            return HP <= 0;
        }

        public bool IsAlive()
        {
            return !IsDead();
        }

        protected float AttackSpeedMultiplier
        {
            get
            {
                var slows = GetComponents<ISlow>();
                var multiplierBySlow = slows.Select(slow => slow.AttackSpeedMultiplier).DefaultIfEmpty(1.0f).Min();
                return multiplierBySlow;
            }
        }

        protected float RunSpeedMultiplier
        {
            get
            {
                var slows = GetComponents<ISlow>();
                var multiplierBySlow = slows.Select(slow => slow.RunSpeedMultiplier).DefaultIfEmpty(1.0f).Min();
                return multiplierBySlow;
            }
        }

        private void Run()
        {
            if (Rooted)
            {
                animator.StopRun();
                return;
            }
            
            animator.Run();

            Vector2 position = transform.position;
            position.x += Time.deltaTime * RunSpeed * RunSpeedMultiplier;
            transform.position = position;
        }

        private IEnumerator Dying()
        {
            StopRun();
            animator.Die();
            
            yield return new WaitForSeconds(1f);

            OnDead();
        }

        protected virtual void Update()
        {
            Root?.Tick();
            if (_hpBar != null)
            {
                _hpBar.UpdatePosition(gameObject, _hudOffset);
            }
        }

        public int CalcAtk()
        {
            var r = ATK * 0.1f;
            return Mathf.FloorToInt((ATK + UnityEngine.Random.Range(-r, r)) * (Power * 0.01f));
        }

        public void UpdateHpBar()
        {
            if (_hpBar == null)
            {
                _hpBar = Widget.Create<ProgressBar>(true);
            }
            _hpBar.UpdatePosition(gameObject, _hudOffset);
            _hpBar.SetText($"{HP} / {HPMax}");
            _hpBar.SetValue((float)HP / (float)HPMax);
        }

        private float GetDamageFactor(AttackType attackType)
        {
            var damageFactorMap = new Dictionary<Tuple<AttackType, WeightType>, float>()
            {
                { new Tuple<AttackType, WeightType>(AttackType.Light, WeightType.Small), 1.25f },
                { new Tuple<AttackType, WeightType>(AttackType.Light, WeightType.Medium), 1.5f },
                { new Tuple<AttackType, WeightType>(AttackType.Light, WeightType.Large), 0.5f },
                { new Tuple<AttackType, WeightType>(AttackType.Light, WeightType.Boss), 0.75f },
                { new Tuple<AttackType, WeightType>(AttackType.Middle, WeightType.Small), 1.0f },
                { new Tuple<AttackType, WeightType>(AttackType.Middle, WeightType.Medium), 1.0f },
                { new Tuple<AttackType, WeightType>(AttackType.Middle, WeightType.Large), 1.25f },
                { new Tuple<AttackType, WeightType>(AttackType.Middle, WeightType.Boss), 0.75f },
                { new Tuple<AttackType, WeightType>(AttackType.Heavy, WeightType.Small), 0.75f },
                { new Tuple<AttackType, WeightType>(AttackType.Heavy, WeightType.Medium), 1.25f },
                { new Tuple<AttackType, WeightType>(AttackType.Heavy, WeightType.Large), 1.5f },
                { new Tuple<AttackType, WeightType>(AttackType.Heavy, WeightType.Boss), 0.75f },
            };
            var factor = damageFactorMap[new Tuple<AttackType, WeightType>(attackType, WeightType)];
            return factor;
        }

        protected int CalcDamage(AttackType attackType, int dmg)
        {
            const float attackDamageFactor = 0.5f;
            const float defenseDamageFactor = 0.25f;
            return Mathf.FloorToInt(
                (attackDamageFactor * dmg - defenseDamageFactor * DEF) *
                GetDamageFactor(attackType)
            );
        }

        public virtual IEnumerator CoProcessDamage(int dmg, bool critical)
        {
            if (dmg <= 0)
                yield break;

            HP -= dmg;

            if (HP > 0)
            {
                animator.Hit();
            }
            else
            {
                StartCoroutine(Dying());
            }

            UpdateHpBar();
        }

        protected virtual void OnDead()
        {
            animator.Idle();
            gameObject.SetActive(false);
        }

        protected virtual void PopUpDmg(Vector3 position, Vector3 force, string dmg, bool critical)
        {
            if (critical)
            {
                ActionCamera.instance.Shake();
                AudioController.PlayDamagedCritical();
                CriticalText.Show(position, force, dmg);
            }
            else
            {
                AudioController.PlayDamaged();
                DamageText.Show(position, force, dmg);
            }
        }

        private void InitBT()
        {
            Root = new Root();
            Root.OpenBranch(
                BT.Selector().OpenBranch(
                    BT.If(CanRun).OpenBranch(
                        BT.Call(Run)
                    ),
                    BT.If(() => !CanRun()).OpenBranch(
                        BT.Call(StopRun)
                    )
                )
            );
        }

        public void StartRun()
        {
            RunSpeed = Speed;
            if (Root == null)
            {
                InitBT();
            }
        }

        protected virtual bool CanRun()
        {
            return !(Mathf.Approximately(RunSpeed, 0f));
        }

        private void AttackEnd(CharacterBase character)
        {
            if (ReferenceEquals(character, this))
                attackEnd = true;
        }

        // FixMe. 캐릭터와 몬스터가 겹치는 현상 있음.
        public bool TargetInRange(CharacterBase target) =>
            Range > Mathf.Abs(gameObject.transform.position.x - target.transform.position.x);

        public void StopRun()
        {
            RunSpeed = 0.0f;
            animator.StopRun();
        }

        public void DisableHUD()
        {
            if (_hpBar != null)
            {
                Destroy(_hpBar.gameObject);
                _hpBar = null;
            }
            if (_castingBar != null)
            {
                Destroy(_castingBar.gameObject);
                _castingBar = null;
            }
        }

        private void ProcessAttack(CharacterBase target, Model.Skill.SkillInfo skill)
        {
            if (TargetInRange(target))
                target.StopRun();
            StartCoroutine(target.CoProcessDamage(skill.Effect, skill.Critical));
        }

        private void ProcessHeal(CharacterBase target, Model.Skill.SkillInfo info)
        {
            var calc = info.Effect - target.HP;
            if (calc <= 0)
            {
                calc = 0;
            }
            target.HP += calc;

            var position = transform.TransformPoint(0f, 1.7f, 0f);
            var force = new Vector3(-0.1f, 0.5f);
            var txt = calc.ToString();
            PopUpHeal(position, force, txt, info.Critical);

            UpdateHpBar();

            Event.OnUpdateStatus.Invoke();
        }

        private void PopUpHeal(Vector3 position, Vector3 force, string dmg, bool critical)
        {
            DamageText.Show(position, force, dmg);

            var pos = transform.position;
            pos.x -= 0.2f;
            pos.y += 0.32f;
            VFXController.instance.Create<BattleHeal01VFX>(pos);
        }

        private IEnumerator CoAnimationAttack()
        {
            attackEnd = false;
            RunSpeed = 0.0f;

            animator.Attack();
            yield return new WaitUntil(() => attackEnd);

            var enemy = GetComponentsInChildren<CharacterBase>()
                .Where(c => c.gameObject.CompareTag(targetTag))
                .OrderBy(c => c.transform.position.x).FirstOrDefault();
            if (enemy != null && !TargetInRange(enemy))
                RunSpeed = Speed;
        }
        public IEnumerator CoAttack(IEnumerable<Model.Skill.SkillInfo> infos)
        {
            yield return StartCoroutine(CoAnimationAttack());

            foreach (var info in infos)
            {
                var target = Game.instance.stage.GetCharacter(info.Target);
                ProcessAttack(target, info);
            }

        }

        public IEnumerator CoHeal(IEnumerable<Model.Skill.SkillInfo> infos)
        {
            yield return StartCoroutine(CoAnimationAttack());

            foreach (var info in infos)
            {
                var target = Game.instance.stage.GetCharacter(info.Target);
                ProcessHeal(target, info);
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                var character = other.gameObject.GetComponent<CharacterBase>();
                if (TargetInRange(character) && character.IsAlive())
                {
                    StopRun();
                }
            }
        }
    }
}
