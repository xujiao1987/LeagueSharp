﻿using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterCommon.M_Orbwalker;

namespace MasterPlugin
{
    class Nasus : Master.Program
    {
        private Int32 Sheen = 3057, Iceborn = 3025;

        public Nasus()
        {
            SkillQ = new Spell(SpellSlot.Q, 300);
            SkillW = new Spell(SpellSlot.W, 600);
            SkillE = new Spell(SpellSlot.E, 650);
            SkillR = new Spell(SpellSlot.R, 20);
            SkillQ.SetSkillshot(0.0435f, 0, 0, false, SkillshotType.SkillshotCircle);
            SkillW.SetTargetted(-0.5f, 0);
            SkillE.SetSkillshot(-0.5f, 0, 20, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("" + Name, Name + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用Q");
                    ItemBool(ComboMenu, "W", "使用W");
                    ItemBool(ComboMenu, "E", "使用E");
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "可杀点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用Q");
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
                    ItemBool(ClearMenu, "E", "使用E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var UltiMenu = new Menu("大招", "Ultimate");
                {
                    ItemBool(UltiMenu, "RSurvive", "使用R保命");
                    ItemSlider(UltiMenu, "RUnder", "血量低于", 30);
                    ChampMenu.AddSubMenu(UltiMenu);
                }
                var MiscMenu = new Menu("杂项", "Misc");
                {
                    ItemBool(MiscMenu, "QLastHit", "使用Q补兵");
                    ItemBool(MiscMenu, "EKillSteal", "使用E抢头");
                    ItemSlider(MiscMenu, "CustomSkin", "换肤", 5, 0, 5).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示", "Draw");
                {
                    ItemBool(DrawMenu, "W", "W范围", false);
                    ItemBool(DrawMenu, "E", "E范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear || Orbwalk.CurrentMode == Orbwalk.Mode.LaneFreeze)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit) LastHit();
            if (ItemBool("Misc", "EKillSteal")) KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "W") && SkillW.Level > 0) Utility.DrawCircle(Player.Position, SkillW.Range, SkillW.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && SkillE.Level > 0) Utility.DrawCircle(Player.Position, SkillE.Range, SkillE.IsReady() ? Color.Green : Color.Red);
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead) return;
            if (sender.IsMe && Orbwalk.IsAutoAttack(args.SData.Name) && IsValid((Obj_AI_Base)args.Target) && SkillQ.IsReady())
            {
                var Obj = (Obj_AI_Base)args.Target;
                var DmgAA = Player.GetAutoAttackDamage(Obj) * Math.Floor(SkillQ.Instance.Cooldown / (1 / (Player.PercentMultiplicativeAttackSpeedMod * 0.638)));
                if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && Obj.Health + 5 <= GetBonusDmg(Obj) && (args.Target is Obj_AI_Minion || args.Target is Obj_AI_Turret))
                {
                    SkillQ.Cast(PacketCast());
                }
                else if (Orbwalk.CurrentMode != Orbwalk.Mode.None && Orbwalk.CurrentMode != Orbwalk.Mode.Flee && Orbwalk.CurrentMode != Orbwalk.Mode.LastHit && (args.Target is Obj_AI_Hero || args.Target is Obj_AI_Minion || args.Target is Obj_AI_Turret) && (Obj.Health + 5 <= GetBonusDmg(Obj) || Obj.Health + 5 > DmgAA + GetBonusDmg(Obj))) SkillQ.Cast(PacketCast());
            }
            else if (sender.IsEnemy && ItemBool("Ultimate", "RSurvive") && SkillR.IsReady())
            {
                if (args.Target.IsMe && ((Orbwalk.IsAutoAttack(args.SData.Name) && (Player.Health - sender.GetAutoAttackDamage(Player, true)) * 100 / Player.MaxHealth <= ItemSlider("Ultimate", "RUnder")) || (args.SData.Name == "summonerdot" && (Player.Health - (sender as Obj_AI_Hero).GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite)) * 100 / Player.MaxHealth <= ItemSlider("Ultimate", "RUnder"))))
                {
                    SkillR.Cast(PacketCast());
                }
                else if ((args.Target.IsMe || (Player.Position.Distance(args.Start) <= args.SData.CastRange[0] && Player.Position.Distance(args.End) <= Orbwalk.GetAutoAttackRange())) && Damage.Spells.ContainsKey((sender as Obj_AI_Hero).ChampionName))
                {
                    for (var i = 3; i > -1; i--)
                    {
                        if (Damage.Spells[(sender as Obj_AI_Hero).ChampionName].FirstOrDefault(a => a.Slot == (sender as Obj_AI_Hero).GetSpellSlot(args.SData.Name, false) && a.Stage == i) != null)
                        {
                            if ((Player.Health - (sender as Obj_AI_Hero).GetSpellDamage(Player, (sender as Obj_AI_Hero).GetSpellSlot(args.SData.Name, false), i)) * 100 / Player.MaxHealth <= ItemSlider("Ultimate", "RUnder")) SkillR.Cast(PacketCast());
                        }
                    }
                }
            }
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (ItemBool("Combo", "W") && SkillW.IsReady() && SkillW.InRange(targetObj.Position)) SkillW.CastOnUnit(targetObj, PacketCast());
            if (ItemBool("Combo", "E") && SkillE.IsReady() && SkillE.InRange(targetObj.Position)) SkillE.Cast(targetObj.Position, PacketCast());
            if (ItemBool("Combo", "Q") && SkillQ.IsReady() && Player.Distance3D(targetObj) <= Orbwalk.GetAutoAttackRange() + 50)
            {
                var DmgAA = Player.GetAutoAttackDamage(targetObj) * Math.Floor(SkillQ.Instance.Cooldown / (1 / (Player.PercentMultiplicativeAttackSpeedMod * 0.638)));
                if (targetObj.Health + 5 <= GetBonusDmg(targetObj) || targetObj.Health + 5 > DmgAA + GetBonusDmg(targetObj))
                {
                    Orbwalk.SetAttack(false);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, targetObj);
                    Orbwalk.SetAttack(true);
                }
            }
            if (ItemBool("Combo", "Item") && Items.CanUseItem(Rand) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Rand);
            if (ItemBool("Combo", "Ignite")) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, SkillE.Range) && (i is Obj_AI_Turret || i is Obj_AI_Minion)).OrderBy(i => i.MaxHealth);
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "E") && SkillE.IsReady() && minionObj.Count(i => i is Obj_AI_Minion) > 0)
                {
                    var posEFarm = SkillE.GetCircularFarmLocation(minionObj.Where(i => i is Obj_AI_Minion).ToList());
                    SkillE.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : minionObj.First(i => i is Obj_AI_Minion).Position.To2D(), PacketCast());
                }
                if (ItemBool("Clear", "Q") && SkillQ.IsReady() && Player.Distance3D(Obj) <= Orbwalk.GetAutoAttackRange() + 50)
                {
                    var DmgAA = Player.GetAutoAttackDamage(Obj) * Math.Floor(SkillQ.Instance.Cooldown / (1 / (Player.PercentMultiplicativeAttackSpeedMod * 0.638)));
                    if (Obj.Health + 5 <= GetBonusDmg(targetObj) || Obj.Health + 5 > DmgAA + GetBonusDmg(Obj))
                    {
                        Orbwalk.SetAttack(false);
                        Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                        Orbwalk.SetAttack(true);
                        break;
                    }
                }
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "QLastHit") || !SkillQ.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, Orbwalk.GetAutoAttackRange() + 50) && i.Health + 5 <= GetBonusDmg(i) && (i is Obj_AI_Minion || i is Obj_AI_Turret)).OrderBy(i => i.MaxHealth).OrderBy(i => i.Distance3D(Player)))
            {
                Orbwalk.SetAttack(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                Orbwalk.SetAttack(true);
                break;
            }
        }

        private void KillSteal()
        {
            if (!SkillE.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => IsValid(i, SkillE.Range) && CanKill(i, SkillE) && i != targetObj).OrderBy(i => i.Health).OrderByDescending(i => i.Distance3D(Player))) SkillE.Cast(Obj.Position, PacketCast());
        }

        private double GetBonusDmg(Obj_AI_Base Target)
        {
            double DmgItem = 0;
            if (Items.HasItem(Sheen) && ((Items.CanUseItem(Sheen) && SkillQ.IsReady()) || Player.HasBuff("sheen", true)) && Player.BaseAttackDamage > DmgItem) DmgItem = Player.BaseAttackDamage;
            if (Items.HasItem(Iceborn) && ((Items.CanUseItem(Iceborn) && SkillQ.IsReady()) || Player.HasBuff("itemfrozenfist", true)) && Player.BaseAttackDamage * 1.25 > DmgItem) DmgItem = Player.BaseAttackDamage * 1.25;
            return (SkillQ.IsReady() ? SkillQ.GetDamage(Target) : 0) + Player.GetAutoAttackDamage(Target, SkillQ.IsReady() ? false : true) + Player.CalcDamage(Target, Damage.DamageType.Physical, DmgItem);
        }
    }
}