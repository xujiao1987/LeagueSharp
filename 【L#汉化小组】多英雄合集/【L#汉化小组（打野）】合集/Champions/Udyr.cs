﻿using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

namespace Master
{
    class Udyr : Program
    {
        private const String Version = "1.0.1";
        private enum Stance
        {
            Tiger,
            Turtle,
            Bear,
            Phoenix
        }
        private Stance CurStance;
        private Int32 AACount = 0;
        private bool TigerActive = false, PhoenixActive = false;
        private Obj_AI_Base minionObj;

        public Udyr()
        {
            SkillQ = new Spell(SpellSlot.Q, 600);
            SkillW = new Spell(SpellSlot.W, 600);
            SkillE = new Spell(SpellSlot.E, 600);
            SkillR = new Spell(SpellSlot.R, 600);

            Config.SubMenu("Orbwalker").SubMenu("lxOrbwalker_Modes").AddItem(new MenuItem(Name + "stunActive", "Stun Cycle").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("连招/骚扰", "csettings"));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "qusage", "使用 Q").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "wusage", "使用 W").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "eusage", "使用 E").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "rusage", "使用 R").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "ignite", "如果可击杀自动使用点燃").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "iusage", "使用项目").SetValue(true));

            Config.AddSubMenu(new Menu("清线/清野", "LaneJungClear"));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearQ", "使用 Q").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearW", "使用 W").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearE", "使用 E").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearR", "使用 R").SetValue(true));

            Config.AddSubMenu(new Menu("杂项", "miscs"));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "useAntiE", "|使用E接近|").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "useInterE", "|使用E打断|").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "surviveW", "|尝试使用W生存|").SetValue(true));

            Game.OnGameUpdate += OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnCreate += OnCreate;
            Obj_AI_Base.OnDelete += OnDelete;
            Game.PrintChat("<font color = \"#33CCCC\">Master of {0}</font> <font color = \"#00ff00\">v{1}</font>", Name, Version);
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
            {
                NormalCombo();
            }
            else if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneFreeze)
            {
                LaneJungClear();
            }
            else if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Flee) Flee();
            if (Config.Item(Name + "stunActive").GetValue<KeyBind>().Active)
            {
                LXOrbwalker.CustomOrbwalkMode = true;
                StunCycle();
            }
            else LXOrbwalker.CustomOrbwalkMode = false;
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item(Name + "useAntiE").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(SkillE.Range) && !gapcloser.Sender.HasBuff("udyrbearstuncheck", true) && (SkillE.IsReady() || CurStance == Stance.Bear))
            {
                if (CurStance != Stance.Bear) SkillE.Cast(PacketCast);
                if (CurStance == Stance.Bear) Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
            }
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item(Name + "useInterE").GetValue<bool>()) return;
            if (unit.IsValidTarget(SkillE.Range) && !unit.HasBuff("udyrbearstuncheck", true) && (SkillE.IsReady() || CurStance == Stance.Bear))
            {
                if (CurStance != Stance.Bear) SkillE.Cast(PacketCast);
                if (CurStance == Stance.Bear) Player.IssueOrder(GameObjectOrder.AttackUnit, unit);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name == SkillQ.Instance.Name)
            {
                CurStance = Stance.Tiger;
                AACount = 0;
            }
            else if (args.SData.Name == SkillW.Instance.Name)
            {
                CurStance = Stance.Turtle;
                AACount = 0;
            }
            else if (args.SData.Name == SkillE.Instance.Name)
            {
                CurStance = Stance.Bear;
                AACount = 0;
            }
            else if (args.SData.Name == SkillR.Instance.Name)
            {
                CurStance = Stance.Phoenix;
                AACount = 0;
            }
            if (LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Flee && LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Lasthit && LXOrbwalker.CurrentMode != LXOrbwalker.Mode.None)
            {
                if (args.SData.Name == Name + "BasicAttack" && (args.Target == targetObj || args.Target == minionObj) && (CurStance == Stance.Tiger || CurStance == Stance.Phoenix)) AACount += 1;
            }
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
            if (Player.Distance(sender.Position) <= 70 && (sender.Name == "Udyr_PhoenixBreath_cas.troy" || sender.Name == "Udyr_Spirit_Phoenix_Breath_cas.troy")) PhoenixActive = true;
            if (Player.Distance(sender.Position) <= 450 && (sender.Name == "udyr_tiger_claw_tar.troy" || sender.Name == "Udyr_Spirit_Tiger_Claw_tar.troy")) TigerActive = true;
            if (sender is Obj_SpellMissile && sender.IsValid && Config.Item(Name + "surviveW").GetValue<bool>() && SkillW.IsReady())
            {
                var missle = (Obj_SpellMissile)sender;
                var caster = missle.SpellCaster;
                if (caster.IsEnemy)
                {
                    var ShieldBuff = new Int32[] { 60, 100, 140, 180, 220 }[SkillW.Level - 1] + 0.5 * Player.FlatMagicDamageMod;
                    if (missle.SData.Name.Contains("BasicAttack"))
                    {
                        if (missle.Target.IsMe && Player.Health <= caster.GetAutoAttackDamage(Player, true) && Player.Health + ShieldBuff > caster.GetAutoAttackDamage(Player, true)) SkillW.Cast();
                    }
                    else if (missle.Target.IsMe || missle.EndPosition.Distance(Player.Position) <= 130)
                    {
                        if (missle.SData.Name == "summonerdot")
                        {
                            if (Player.Health <= (caster as Obj_AI_Hero).GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite) && Player.Health + ShieldBuff > (caster as Obj_AI_Hero).GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite)) SkillW.Cast();
                        }
                        else if (Player.Health <= (caster as Obj_AI_Hero).GetSpellDamage(Player, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name, false), 1) && Player.Health + ShieldBuff > (caster as Obj_AI_Hero).GetSpellDamage(Player, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name, false), 1)) SkillW.Cast();
                    }
                }
            }
        }

        private void OnDelete(GameObject sender, EventArgs args)
        {
            if (Player.Distance(sender.Position) <= 70 && (sender.Name == "Udyr_PhoenixBreath_cas.troy" || sender.Name == "Udyr_Spirit_Phoenix_Breath_cas.troy")) PhoenixActive = false;
            if (Player.Distance(sender.Position) <= 450 && (sender.Name == "udyr_tiger_claw_tar.troy" || sender.Name == "Udyr_Spirit_Tiger_Claw_tar.troy")) TigerActive = false;
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (Config.Item(Name + "eusage").GetValue<bool>() && SkillE.IsReady() && !targetObj.HasBuff("udyrbearstuncheck", true)) SkillE.Cast(PacketCast);
            if (targetObj.IsValidTarget(400) && (!Config.Item(Name + "eusage").GetValue<bool>() || (Config.Item(Name + "eusage").GetValue<bool>() && (SkillE.Level == 0 || (SkillE.Level >= 1 && targetObj.HasBuff("udyrbearstuncheck", true))))))
            {
                if (Config.Item(Name + "qusage").GetValue<bool>() && SkillQ.IsReady()) SkillQ.Cast(PacketCast);
                if (Config.Item(Name + "rusage").GetValue<bool>() && SkillR.IsReady() && SkillQ.Level >= 1 && CurStance == Stance.Tiger && (AACount >= 2 || TigerActive))
                {
                    SkillR.Cast(PacketCast);
                }
                else if (SkillQ.Level == 0 && SkillR.IsReady()) SkillR.Cast(PacketCast);
                if (Config.Item(Name + "wusage").GetValue<bool>() && SkillW.IsReady())
                {
                    if (CurStance == Stance.Phoenix && (AACount >= 3 || PhoenixActive))
                    {
                        SkillW.Cast(PacketCast);
                    }
                    else if (CurStance == Stance.Tiger && (AACount >= 2 || TigerActive))
                    {
                        SkillW.Cast(PacketCast);
                    }
                    else if (SkillQ.Level == 0 && SkillR.Level == 0) SkillW.Cast(PacketCast);
                }
            }
            if (Config.Item(Name + "iusage").GetValue<bool>()) UseItem(targetObj);
            if (Config.Item(Name + "ignite").GetValue<bool>()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            minionObj = MinionManager.GetMinions(Player.Position, SkillE.Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault();
            LXOrbwalker.ForcedTarget = minionObj;
            if (minionObj == null) return;
            if (Config.Item(Name + "useClearE").GetValue<bool>() && SkillE.IsReady() && !minionObj.HasBuff("udyrbearstuncheck", true)) SkillE.Cast(PacketCast);
            if (minionObj.IsValidTarget(400) && (!Config.Item(Name + "useClearE").GetValue<bool>() || (Config.Item(Name + "useClearE").GetValue<bool>() && (SkillE.Level == 0 || (SkillE.Level >= 1 && minionObj.HasBuff("udyrbearstuncheck", true))))))
            {
                if (Config.Item(Name + "useClearQ").GetValue<bool>() && SkillQ.IsReady()) SkillQ.Cast(PacketCast);
                if (Config.Item(Name + "useClearR").GetValue<bool>() && SkillR.IsReady() && SkillQ.Level >= 1 && CurStance == Stance.Tiger && (AACount >= 2 || TigerActive))
                {
                    SkillR.Cast(PacketCast);
                }
                else if (SkillQ.Level == 0 && SkillR.IsReady()) SkillR.Cast(PacketCast);
                if (Config.Item(Name + "useClearW").GetValue<bool>() && SkillW.IsReady())
                {
                    if (CurStance == Stance.Phoenix && (AACount >= 3 || PhoenixActive))
                    {
                        SkillW.Cast(PacketCast);
                    }
                    else if (CurStance == Stance.Tiger && (AACount >= 2 || TigerActive))
                    {
                        SkillW.Cast(PacketCast);
                    }
                    else if (SkillQ.Level == 0 && SkillR.Level == 0) SkillW.Cast(PacketCast);
                }
            }
        }

        private void Flee()
        {
            var manaQ = SkillQ.Instance.ManaCost;
            var manaW = SkillW.Instance.ManaCost;
            var manaR = SkillR.Instance.ManaCost;
            var PData = Player.Buffs.FirstOrDefault(i => i.Name == "udyrmonkeyagilitybuff");
            if (SkillE.IsReady()) SkillE.Cast(PacketCast);
            if (PData != null && PData.Count < 3)
            {
                if ((manaQ < manaW || manaQ < manaR || (manaQ == manaW && manaQ < manaR) || (manaQ == manaR && manaQ < manaW)) && SkillQ.IsReady())
                {
                    SkillQ.Cast(PacketCast);
                }
                else if ((manaW < manaQ || manaW < manaR || (manaW == manaQ && manaW < manaR) || (manaW == manaR && manaW < manaQ)) && SkillW.IsReady())
                {
                    SkillW.Cast(PacketCast);
                }
                else if ((manaR < manaQ || manaR < manaW || (manaR == manaQ && manaR < manaW) || (manaR == manaW && manaR < manaQ)) && SkillR.IsReady()) SkillR.Cast(PacketCast);
            }
        }

        private void StunCycle()
        {
            var targetClosest = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(SkillE.Range) && !i.HasBuff("udyrbearstuncheck", true)).OrderBy(i => i.Distance(Player)).FirstOrDefault();
            Orbwalk(targetClosest);
            if (targetClosest == null) return;
            if (SkillE.IsReady()) SkillE.Cast(PacketCast);
        }

        private void UseItem(Obj_AI_Hero target)
        {
            if (Items.CanUseItem(Bilge) && Player.Distance(target) <= 450) Items.UseItem(Bilge, target);
            if (Items.CanUseItem(Blade) && Player.Distance(target) <= 450) Items.UseItem(Blade, target);
            if (Items.CanUseItem(Rand) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Rand);
        }
    }
}