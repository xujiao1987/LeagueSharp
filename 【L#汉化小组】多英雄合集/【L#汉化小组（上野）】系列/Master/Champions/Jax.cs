﻿using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterCommon.M_Orbwalker;

namespace MasterPlugin
{
    class Jax : Master.Program
    {
        private Int32 Sheen = 3057, Trinity = 3078;
        private bool WardCasted = false, ECasted = false;
        private int RCount = 0;

        public Jax()
        {
            SkillQ = new Spell(SpellSlot.Q, 700);
            SkillW = new Spell(SpellSlot.W, 300);
            SkillE = new Spell(SpellSlot.E, 187.5f);
            SkillR = new Spell(SpellSlot.R, 100);
            SkillQ.SetTargetted(-0.5f, 0);
            SkillW.SetSkillshot(0.0435f, 0, 0, false, SkillshotType.SkillshotCircle);
            SkillE.SetSkillshot(0, 0, 1450, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("" + Name, Name + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用Q");
                    ItemBool(ComboMenu, "W", "使用W");
                    ItemList(ComboMenu, "WMode", "模式", new[] { "平A之后", "R之后" });
                    ItemBool(ComboMenu, "E", "使用E");
                    ItemBool(ComboMenu, "R", "使用R");
                    ItemList(ComboMenu, "RMode", "模式e", new[] { "血量", "敌人大于" });
                    ItemSlider(ComboMenu, "RUnder", "血量低于", 40);
                    ItemSlider(ComboMenu, "RCount", "敌人大于", 2, 1, 4);
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "可杀点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用Q");
                    ItemSlider(HarassMenu, "QAbove", "血量大于", 20);
                    ItemBool(HarassMenu, "W", "使用W");
                    ItemBool(HarassMenu, "E", "使用E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线", "Clear");
                {
                    var SmiteMob = new Menu("自动惩戒", "SmiteMob");
                    {
                        ItemBool(SmiteMob, "Baron", "大龙");
                        ItemBool(SmiteMob, "Dragon", "小龙");
                        ItemBool(SmiteMob, "Red", "红BUFF");
                        ItemBool(SmiteMob, "Blue", "蓝BUFF");
                        ItemBool(SmiteMob, "Krug", "石像");
                        ItemBool(SmiteMob, "Gromp", "幽灵");
                        ItemBool(SmiteMob, "Raptor", "F4");
                        ItemBool(SmiteMob, "Wolf", "三狼");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    ItemBool(ClearMenu, "Q", "使用Q");
                    ItemBool(ClearMenu, "W", "使用W");
                    ItemList(ClearMenu, "WMode", "模式", new[] { "平A之后", "R之后" });
                    ItemBool(ClearMenu, "E", "使用E");
                    ItemBool(ClearMenu, "Item", "使用提亚马特/九头蛇");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("杂项", "Misc");
                {
                    ItemBool(MiscMenu, "WLastHit", "使用W补兵");
                    ItemBool(MiscMenu, "WQKillSteal", "使用WQ抢头");
                    ItemBool(MiscMenu, "EAntiGap", "使用E防突");
                    ItemBool(MiscMenu, "EInterrupt", "使用E打断");
                    ItemBool(MiscMenu, "RSurvive", "使用R保命");
                    ItemSlider(MiscMenu, "CustomSkin", "换肤", 8, 0, 8).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q范围", false);
                    ItemBool(DrawMenu, "E", "E范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
               
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Minion.OnCreate += OnCreateObjMinion;
            Orbwalk.AfterAttack += AfterAttack;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen)
            {
                if (Player.IsDead) RCount = 0;
                return;
            }
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalk.Mode.Combo:
                    NormalCombo();
                    break;
                case Orbwalk.Mode.Harass:
                    Harass();
                    break;
                case Orbwalk.Mode.LaneClear:
                    LaneJungClear();
                    break;
                case Orbwalk.Mode.LaneFreeze:
                    LaneJungClear();
                    break;
                case Orbwalk.Mode.LastHit:
                    LastHit();
                    break;
                case Orbwalk.Mode.Flee:
                    WardJump(Game.CursorPos);
                    break;
            }
            if (ItemBool("Misc", "WQKillSteal")) KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "Q") && SkillQ.Level > 0) Utility.DrawCircle(Player.Position, SkillQ.Range, SkillQ.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && SkillE.Level > 0) Utility.DrawCircle(Player.Position, SkillE.Range, SkillE.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ItemBool("Misc", "EAntiGap")) return;
            if (IsValid(gapcloser.Sender, SkillE.Range + 10) && SkillE.IsReady()) SkillE.Cast(PacketCast());
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!ItemBool("Misc", "EInterrupt")) return;
            if (Player.Mana >= SkillQ.Instance.ManaCost + SkillE.Instance.ManaCost && !SkillE.InRange(unit.Position) && IsValid(unit, SkillQ.Range)) SkillQ.CastOnUnit(unit, PacketCast());
            if (IsValid(unit, SkillE.Range) && SkillE.IsReady()) SkillE.Cast(PacketCast());
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead) return;
            if (sender.IsMe)
            {
                if (Orbwalk.IsAutoAttack(args.SData.Name) && IsValid((Obj_AI_Base)args.Target) && (args.Target as Obj_AI_Base).Health + 5 <= GetBonusDmg((Obj_AI_Base)args.Target) && Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && SkillW.IsReady() && args.Target is Obj_AI_Minion) SkillW.Cast(PacketCast());
                if (args.SData.Name == "JaxCounterStrike")
                {
                    ECasted = true;
                    Utility.DelayAction.Add(1800, () => ECasted = false);
                }
                if (args.SData.Name == "jaxrelentlessattack")
                {
                    RCount = 0;
                    if (SkillW.IsReady() && IsValid((Obj_AI_Base)args.Target, Orbwalk.GetAutoAttackRange() + 50))
                    {
                        switch (Orbwalk.CurrentMode)
                        {
                            case Orbwalk.Mode.Combo:
                                if (ItemBool("Combo", "W") && ItemList("Combo", "WMode") == 1) SkillW.Cast(PacketCast());
                                break;
                            case Orbwalk.Mode.LaneClear:
                                if (ItemBool("Clear", "W") && ItemList("Clear", "WMode") == 1) SkillW.Cast(PacketCast());
                                break;
                            case Orbwalk.Mode.LaneFreeze:
                                if (ItemBool("Clear", "W") && ItemList("Clear", "WMode") == 1) SkillW.Cast(PacketCast());
                                break;
                        }
                    }
                }
            }
            else if (sender.IsEnemy && ItemBool("Misc", "RSurvive") && SkillR.IsReady())
            {
                if (args.Target.IsMe && (Orbwalk.IsAutoAttack(args.SData.Name) && Player.Health <= sender.GetAutoAttackDamage(Player, true)))
                {
                    SkillR.Cast(PacketCast());
                }
                else if ((args.Target.IsMe || (Player.Position.Distance(args.Start) <= args.SData.CastRange[0] && Player.Position.Distance(args.End) <= Orbwalk.GetAutoAttackRange())) && Damage.Spells.ContainsKey((sender as Obj_AI_Hero).ChampionName))
                {
                    for (var i = 3; i > -1; i--)
                    {
                        if (Damage.Spells[(sender as Obj_AI_Hero).ChampionName].FirstOrDefault(a => a.Slot == (sender as Obj_AI_Hero).GetSpellSlot(args.SData.Name, false) && a.Stage == i) != null)
                        {
                            if (Player.Health <= (sender as Obj_AI_Hero).GetSpellDamage(Player, (sender as Obj_AI_Hero).GetSpellSlot(args.SData.Name, false), i)) SkillR.Cast(PacketCast());
                        }
                    }
                }
            }
        }

        private void OnCreateObjMinion(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || sender.IsEnemy || Player.IsDead || !SkillQ.IsReady() || !WardCasted) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && Player.Distance3D((Obj_AI_Minion)sender) <= SkillQ.Range + sender.BoundingRadius && sender.Name.EndsWith("Ward"))
            {
                SkillQ.CastOnUnit((Obj_AI_Minion)sender, PacketCast());
                return;
            }
        }

        private void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe) return;
            RCount += 1;
            if (SkillW.IsReady() && IsValid(target, Orbwalk.GetAutoAttackRange() + 50))
            {
                switch (Orbwalk.CurrentMode)
                {
                    case Orbwalk.Mode.Combo:
                        if (ItemBool("Combo", "W") && ItemList("Combo", "WMode") == 0) SkillW.Cast(PacketCast());
                        break;
                    case Orbwalk.Mode.Harass:
                        if (ItemBool("Harass", "W") && (!ItemBool("Harass", "Q") || (ItemBool("Harass", "Q") && !SkillQ.IsReady()))) SkillW.Cast(PacketCast());
                        break;
                    case Orbwalk.Mode.LaneClear:
                        if (ItemBool("Clear", "W") && ItemList("Clear", "WMode") == 0) SkillW.Cast(PacketCast());
                        break;
                    case Orbwalk.Mode.LaneFreeze:
                        if (ItemBool("Clear", "W") && ItemList("Clear", "WMode") == 0) SkillW.Cast(PacketCast());
                        break;
                }
            }
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (ItemBool("Combo", "E") && SkillE.IsReady())
            {
                if (!Player.HasBuff("JaxCounterStrike", true))
                {
                    if ((ItemBool("Combo", "Q") && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position)) || SkillE.InRange(targetObj.Position)) SkillE.Cast(PacketCast());
                }
                else if (SkillE.InRange(targetObj.Position) && !IsValid(targetObj, SkillE.Range - 3.5f)) SkillE.Cast(PacketCast());
            }
            if (ItemBool("Combo", "Q") && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position))
            {
                if ((ItemBool("Combo", "E") && SkillE.IsReady() && Player.HasBuff("JaxCounterStrike", true) && !SkillE.InRange(targetObj.Position)) || (!Orbwalk.InAutoAttackRange(targetObj) && Player.Distance3D(targetObj) > 450)) SkillQ.CastOnUnit(targetObj, PacketCast());
            }
            if (ItemBool("Combo", "R") && SkillR.IsReady())
            {
                switch (ItemList("Combo", "RMode"))
                {
                    case 0:
                        if (Player.Health * 100 / Player.MaxHealth <= ItemSlider("Combo", "RUnder")) SkillR.Cast(PacketCast());
                        break;
                    case 1:
                        if (Player.CountEnemysInRange((int)SkillQ.Range) >= ItemSlider("Combo", "RCount")) SkillR.Cast(PacketCast());
                        break;
                }
            }
            if (ItemBool("Combo", "Item")) UseItem(targetObj);
            if (ItemBool("Combo", "Ignite")) CastIgnite(targetObj);
        }

        private void Harass()
        {
            if (targetObj == null) return;
            if (ItemBool("Harass", "W") && SkillW.IsReady() && ItemBool("Harass", "Q") && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position)) SkillW.Cast(PacketCast());
            if (ItemBool("Harass", "E") && SkillE.IsReady())
            {
                if (!Player.HasBuff("JaxCounterStrike", true))
                {
                    if ((ItemBool("Harass", "Q") && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position)) || SkillE.InRange(targetObj.Position)) SkillE.Cast(PacketCast());
                }
                else if (SkillE.InRange(targetObj.Position) && !IsValid(targetObj, SkillE.Range - 3.5f)) SkillE.Cast(PacketCast());
            }
            if (ItemBool("Harass", "Q") && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position) && Player.Health * 100 / Player.MaxHealth >= ItemList("Harass", "QAbove"))
            {
                if ((ItemBool("Harass", "E") && SkillE.IsReady() && Player.HasBuff("JaxCounterStrike", true) && !SkillE.InRange(targetObj.Position)) || (!Orbwalk.InAutoAttackRange(targetObj) && Player.Distance3D(targetObj) > 450)) SkillQ.CastOnUnit(targetObj, PacketCast());
            }
        }

        private void LaneJungClear()
        {
            foreach (var Obj in ObjectManager.Get<Obj_AI_Minion>().Where(i => IsValid(i, SkillQ.Range)).OrderBy(i => i.Health))
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "E") && SkillE.IsReady())
                {
                    if (!Player.HasBuff("JaxCounterStrike", true))
                    {
                        if ((ItemBool("Clear", "Q") && SkillQ.IsReady()) || SkillE.InRange(Obj.Position)) SkillE.Cast(PacketCast());
                    }
                    else if (SkillE.InRange(Obj.Position) && !ECasted) SkillE.Cast(PacketCast());
                }
                if (ItemBool("Clear", "Q") && SkillQ.IsReady() && ((ItemBool("Clear", "E") && SkillE.IsReady() && Player.HasBuff("JaxCounterStrike", true) && !SkillE.InRange(Obj.Position)) || (!Orbwalk.InAutoAttackRange(Obj) && Player.Distance3D(Obj) > 450))) SkillQ.CastOnUnit(Obj, PacketCast());
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "WLastHit") || !SkillW.IsReady() || !Player.HasBuff("JaxEmpowerTwo", true)) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Minion>().Where(i => IsValid(i, Orbwalk.GetAutoAttackRange() + 50) && i.Health + 5 <= GetBonusDmg(i)).OrderBy(i => i.Health).OrderBy(i => i.Distance3D(Player)))
            {
                Orbwalk.SetAttack(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                Orbwalk.SetAttack(true);
                break;
            }
        }

        private void WardJump(Vector3 Pos)
        {
            if (!SkillQ.IsReady()) return;
            bool IsWard = false;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, SkillQ.Range + i.BoundingRadius, false) && !(i is Obj_AI_Turret) && i.Position.Distance(Pos) < 230).OrderBy(i => i.Position.Distance(Pos)))
            {
                SkillQ.CastOnUnit(Obj, PacketCast());
                if (Obj.Name.EndsWith("Ward") && Obj.IsMinion)
                {
                    IsWard = true;
                }
                else return;
            }
            if (!IsWard && (GetWardSlot() != null || GetWardSlot().Stacks > 0) && !WardCasted)
            {
                GetWardSlot().UseItem(Player.Position.Distance(Pos) > GetWardRange(GetWardSlot().Id) ? Player.Position.To2D().Extend(Pos.To2D(), GetWardRange(GetWardSlot().Id)).To3D() : Pos);
                WardCasted = true;
                Utility.DelayAction.Add(1000, () => WardCasted = false);
            }
        }

        private void KillSteal()
        {
            if (Player.Mana < SkillQ.Instance.ManaCost + SkillW.Instance.ManaCost) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => IsValid(i, SkillQ.Range) && i.Health + 5 <= SkillQ.GetDamage(i) + GetBonusDmg(i) && i != targetObj).OrderBy(i => i.Health).OrderByDescending(i => i.Distance3D(Player)))
            {
                if (SkillW.IsReady()) SkillW.Cast(PacketCast());
                if (SkillQ.IsReady() && Player.HasBuff("JaxEmpowerTwo", true)) SkillQ.CastOnUnit(Obj, PacketCast());
            }
        }

        private void UseItem(Obj_AI_Hero Target, bool Farm = false)
        {
            if (Items.CanUseItem(Bilge) && Player.Distance3D(Target) <= 450 && !Farm) Items.UseItem(Bilge, Target);
            if (Items.CanUseItem(Blade) && Player.Distance3D(Target) <= 450 && !Farm) Items.UseItem(Blade, Target);
            if (Items.CanUseItem(Tiamat) && Farm ? Player.Distance3D(Target) <= 350 : Player.CountEnemysInRange(350) >= 1) Items.UseItem(Tiamat);
            if (Items.CanUseItem(Hydra) && Farm ? Player.Distance3D(Target) <= 350 : (Player.CountEnemysInRange(350) >= 2 || (Player.GetAutoAttackDamage(Target) < Target.Health && Player.CountEnemysInRange(350) == 1))) Items.UseItem(Hydra);
            if (Items.CanUseItem(Rand) && Player.CountEnemysInRange(450) >= 1 && !Farm) Items.UseItem(Rand);
        }

        private double GetBonusDmg(Obj_AI_Base Target)
        {
            double DmgItem = 0;
            if (Items.HasItem(Sheen) && ((Items.CanUseItem(Sheen) && SkillW.IsReady()) || Player.HasBuff("sheen", true)) && Player.BaseAttackDamage > DmgItem) DmgItem = Player.BaseAttackDamage;
            if (Items.HasItem(Trinity) && ((Items.CanUseItem(Trinity) && SkillW.IsReady()) || Player.HasBuff("sheen", true)) && Player.BaseAttackDamage * 2 > DmgItem) DmgItem = Player.BaseAttackDamage * 2;
            return (SkillW.IsReady() || Player.HasBuff("JaxEmpowerTwo", true) ? SkillW.GetDamage(Target) : 0) + (RCount >= 2 ? SkillR.GetDamage(Target) : 0) + Player.GetAutoAttackDamage(Target, true) + Player.CalcDamage(Target, Damage.DamageType.Physical, DmgItem);
        }
    }
}