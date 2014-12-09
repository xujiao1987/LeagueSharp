﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace DZDraven_Reloaded
{
    class DZDraven_Reloaded
    {
        private static String ChampName = "Draven";
        public static Menu Menu;
        public static Spell Q, W, E, R;
        private static xSLxOrbwalker xSLx;
        private static Obj_AI_Hero Player;
        private static List<PossibleReticle> Axes = new List<PossibleReticle>(); 

        public static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if(Player.ChampionName != "Draven")return;

            Menu = new Menu("德莱文", "DravenReloaded", true);
            var xSLxMenu = new Menu("走砍", "Orbwalker1");
            xSLxOrbwalker.AddToMenu(xSLxMenu);
            Menu.AddSubMenu(xSLxMenu);
            var ts = new Menu("目标选择", "TargetSelector");
            SimpleTs.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            Menu.AddSubMenu(new Menu("连招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "使用Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseWC", "使用W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "使用E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "使用R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "Q蓝量").SetValue(new Slider(30, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("WManaC", "W蓝量").SetValue(new Slider(25, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "E蓝量").SetValue(new Slider(20, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("RManaC", "R蓝量").SetValue(new Slider(5, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CAC", "接住斧头").SetValue(true));

            Menu.AddSubMenu(new Menu("骚扰", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "使用Q").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseWH", "使用W").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "使用E").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseRH", "使用R").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "Q蓝量").SetValue(new Slider(30, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("WManaH", "W蓝量").SetValue(new Slider(25, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "E蓝量").SetValue(new Slider(20, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("RManaH", "R蓝量").SetValue(new Slider(5, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("CAH", "接住斧头").SetValue(true));

            Menu.AddSubMenu(new Menu("补兵", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "使用Q").SetValue(true));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseELC", "使用E").SetValue(true));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "Q蓝量").SetValue(new Slider(25, 1, 100)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("EManaLC", "E蓝量").SetValue(new Slider(25, 1, 100)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("CAF", "接住斧头").SetValue(true));

            Menu.AddSubMenu(new Menu("斧头", "Axes"));
            Menu.SubMenu("Axes").AddItem(new MenuItem("MaxAxeC", "连招最大斧头数").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes").AddItem(new MenuItem("MaxAxeH", "骚扰最大斧头数").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes").AddItem(new MenuItem("MaxAxeF", "补兵最大斧头数").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes")
                .AddItem(new MenuItem("CatchRadius", "半径").SetValue(new Slider(600, 200, 1000)));
            Menu.SubMenu("Axes")
                .AddItem(new MenuItem("SafeZone", "安全区域").SetValue(new Slider(125, 0, 325)));
            Menu.AddSubMenu(new Menu("杂项", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("ManualR", "手动R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "封包").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "防突")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "打断").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WCatch", "W接住斧头").SetValue(true));

            Menu.AddSubMenu(new Menu("物品", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkC", "破败(连招)").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkH", "破败(骚扰)").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuC", "幽梦(连招)").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuH", "幽梦(骚扰)").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeC", "小弯刀(连招)").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeH", "小弯刀(骚扰)").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "破败HP").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "破败敌人HP").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("净化选项", "QSSMenu"));
            Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "水银饰带").SetValue(true));
            Menu.AddSubMenu(new Menu("净化类型|", "QSST"));
            Cleanser.CreateTypeQSSMenu();
            Menu.AddSubMenu(new Menu("净化技能|", "QSSSpell"));
            Cleanser.CreateQSSSpellMenu();

            Menu.AddSubMenu(new Menu("显示", "Drawing"));

            //Drawings Menu
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E范围").SetValue(new Circle(true, Color.MediumPurple)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawCRange", "斧头范围").SetValue(new Circle(true, Color.RoyalBlue)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawRet", "显示斧头").SetValue(new Circle(true, Color.Yellow)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawNextRet", "显示下个斧头").SetValue(new Circle(true, Color.Orange)));
            Menu.AddToMainMenu();

			
		
			
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            xSLxOrbwalker.AfterAttack += xSLxOrbwalker_AfterAttack;
            Game.PrintChat("<font color='#FF0000'>DZDraven</font> ReLoaded!");
            Game.PrintChat("By <font color='#FF0000'>DZ</font><font color='#FFFFFF'>191</font>. Special Thanks to: Lexxes");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");
        }

        static void xSLxOrbwalker_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            //Game.PrintChat("Registered");
            if (!(target is Obj_AI_Hero)) return;
            if (unit.IsMe && target.IsValidTarget())
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                CastW(target);
                castItems((Obj_AI_Hero)target);
            }
        }


        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            minionThere();
            //Game.PrintChat(hasWBuff().ToString());
            var target = SimpleTs.GetTarget(xSLxOrbwalker.GetAutoAttackRange(), SimpleTs.DamageType.Physical);
            var Etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var RTarget = SimpleTs.GetTarget(2000f, SimpleTs.DamageType.Physical);
            Cleanser.cleanserBySpell();
            Cleanser.cleanserByBuffType();
            CatchAxes();
            if(target.IsValidTarget())CastQ();
            if(Etarget.IsValidTarget())CastE(Etarget);
            if (RTarget.IsValidTarget()) CastRExecute(RTarget);
            if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.LaneClear) { EFarmCheck();}
            if (RTarget.IsValidTarget() && Menu.Item("ManualR").GetValue<KeyBind>().Active) { RExecute(RTarget);}
        }


        static void Drawing_OnDraw(EventArgs args)
        {
            var DrawCatch = Menu.Item("DrawCRange").GetValue<Circle>();
            var radius = Menu.Item("CatchRadius").GetValue<Slider>().Value;
            if (DrawCatch.Active) Utility.DrawCircle(Game.CursorPos, radius, DrawCatch.Color);
            var DrawE = Menu.Item("DrawE").GetValue<Circle>();
            if (DrawE.Active) Utility.DrawCircle(Player.Position, E.Range, DrawE.Color);
            var DrawRet = Menu.Item("DrawRet").GetValue<Circle>();
             var DrawNextRet = Menu.Item("DrawNextRet").GetValue<Circle>();
            bool shouldUseW;
            var NextRet = getClosestAxe(out shouldUseW);
            
            if (DrawRet.Active && Axes.Count > 0)
            {
                foreach (PossibleReticle r in Axes.Where(ret => Game.CursorPos.Distance(ret.Position)<=radius && ret !=NextRet))
                {
                    Utility.DrawCircle(r.Position,100f,DrawRet.Color);
                }
            }
            if(DrawNextRet.Active && NextRet != null)Utility.DrawCircle(NextRet.Position,100f,DrawNextRet.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var GPSender = (Obj_AI_Hero)gapcloser.Sender;
            if (!isMenuEnabled("AntiGP") || !E.IsReady() || !GPSender.IsValidTarget()) return;
            CastEHitchance(GPSender);

        }

        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var Sender = (Obj_AI_Hero)unit;
            if (!isMenuEnabled("Interrupt") || !E.IsReady() || !Sender.IsValidTarget()) return;
            CastEHitchance(Sender);
        }

        static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self")) return;
            IEnumerable<PossibleReticle> ret = Axes.Where(a => a.networkID == sender.NetworkId);
            foreach (var axe in ret)
            {
                Axes.Remove(axe);
            }
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self"))return;
            Axes.Add(new PossibleReticle(sender));
        }

        public static PossibleReticle getClosestAxe(out bool useW)
        {
            if (Axes.Count <= 0)
            {
                useW = false;
                return null;
            }
            var CatchRange = Menu.Item("CatchRadius").GetValue<Slider>().Value;
            var UseW = isMenuEnabled("WCatch");
            bool ShouldUseW;
            var Axe =Axes.Where(
                    axe =>
                        axe.AxeGameObject.IsValid && axe.Position.Distance(Game.CursorPos) <= CatchRange).OrderBy(axe => axe.Distance())
                        .First();
            if (Axe.canCatch(UseW, out ShouldUseW))
            {
                useW = ShouldUseW;
                return Axe;
            }
            useW = false;
            return null;
        }

        public static void CatchAxes()
        {
            bool shouldUseWForIt;
            //Game.PrintChat("I'm Combo");
            var Axe = getClosestAxe(out shouldUseWForIt);
            
            if (Axe == null)
            {
                xSLxOrbwalker.CustomOrbwalkMode = false;
                xSLxOrbwalker.SetAttack(true);
                return;
            }
            if (shouldUseWForIt) { xSLxOrbwalker.SetAttack(false); } else { xSLxOrbwalker.SetAttack(true);}
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    var catchCombo = isMenuEnabled("CAC");
                    if (!catchCombo) return;
                    Catch(shouldUseWForIt, Axe);
                  break;
                case xSLxOrbwalker.Mode.Harass:
                  var catchHarass = isMenuEnabled("CAH");
                    if (!catchHarass) return;
                    Catch(shouldUseWForIt, Axe);
                  break;
                case xSLxOrbwalker.Mode.Lasthit:
                  var catchLastHit = isMenuEnabled("CAF");
                  if (!catchLastHit) return;
                  Catch(shouldUseWForIt, Axe);
                  break;
                case xSLxOrbwalker.Mode.LaneClear:
                  var catchLaneClear = isMenuEnabled("CAF");
                  if (!catchLaneClear) return;
                    Catch(shouldUseWForIt,Axe);
                  break;
            }
        }

        public static void CastEHitchance(Obj_AI_Hero target)
        {
            var Pred = E.GetPrediction(target);
            if (Pred.Hitchance >= HitChance.Medium)
            {
                E.Cast(target, isMenuEnabled("Packets"));
            }
        }
        public static void Catch(bool shouldUseWForIt, PossibleReticle Axe)
        {
            if (shouldUseWForIt && W.IsReady() && !Axe.isCatchingNow()) W.Cast();
            xSLxOrbwalker.CustomOrbwalkMode = true;
            xSLxOrbwalker.Orbwalk(PosAfterRange(Axe.Position, Game.CursorPos, 49 + Player.BoundingRadius / 2), xSLxOrbwalker.GetPossibleTarget()); 
        }
        public static void CastQ()
        { 
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!isMenuEnabled("UseQC")) return;
                    var ManaQCombo = Menu.Item("QManaC").GetValue<Slider>().Value;
                    var QMax = Menu.Item("MaxAxeC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaQCombo && GetQStacks() + 1 <= QMax) Q.Cast(isMenuEnabled("Packets"));
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!isMenuEnabled("UseQC")) return;
                    var ManaQHarass = Menu.Item("QManaH").GetValue<Slider>().Value;
                    var QMaxH = Menu.Item("MaxAxeH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaQHarass && GetQStacks() + 1 <= QMaxH) Q.Cast(isMenuEnabled("Packets"));
                    break;
                case xSLxOrbwalker.Mode.Lasthit:
                    if (!isMenuEnabled("UseQF")) return;
                    var ManaQLH = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    var QMaxLH = Menu.Item("MaxAxeF").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaQLH && GetQStacks() + 1 <= QMaxLH && minionThere()) Q.Cast(isMenuEnabled("Packets"));
                    break;
                case xSLxOrbwalker.Mode.LaneClear:
                    if (!isMenuEnabled("UseQLC")) return;
                    var ManaQLC = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    var QLC = Menu.Item("MaxAxeF").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaQLC && GetQStacks() + 1 <= QLC && minionThere()) Q.Cast(isMenuEnabled("Packets"));
                    break;
            }
        }
        private static void CastW(Obj_AI_Base target)
        {
            if (hasWBuff() || !W.IsReady()) return;
            
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!isMenuEnabled("UseWC")) return;
                    var MWC = Menu.Item("WManaC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= MWC) W.Cast(isMenuEnabled("Packets"));
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!isMenuEnabled("UseWC")) return;
                    var MWH = Menu.Item("WManaH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= MWH) W.Cast(isMenuEnabled("Packets"));
                    break;
            }
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !target.IsValidTarget()) return;

            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!isMenuEnabled("UseEC")) return;
                    var MEC = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= MEC) CastEHitchance(target);
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!isMenuEnabled("UseEH")) return;
                    var MEH = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= MEH) CastEHitchance(target);
                    break;
            }
        }

        private static void EFarmCheck()
        {
            if (!isMenuEnabled("UseEF")) return;
            List<Obj_AI_Base> MinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            MinionManager.FarmLocation EFarmLocation = E.GetLineFarmLocation(MinionsE);
            var MELC = Menu.Item("EManaLC").GetValue<Slider>().Value;
            if (getPerValue(true) >= MELC && EFarmLocation.MinionsHit > 2)
            {
                E.Cast(EFarmLocation.Position, isMenuEnabled("Packets"));
            }
        }
        private static void CastRExecute(Obj_AI_Hero RTarget)
        {
            var Pred = R.GetPrediction(RTarget);
            if (!RTarget.IsValidTarget() || Pred.Hitchance < HitChance.Medium || !R.IsReady()) return;
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!isMenuEnabled("UseRC")) return;
                    var ManaR = Menu.Item("RManaC").GetValue<Slider>().Value;
                    if (getUnitsInPath(Player, RTarget, R) && getPerValue(true) >= ManaR &&!Player.HasBuff("dravenrdoublecast", true))
                    {
                        R.Cast(RTarget, isMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!isMenuEnabled("UseRH")) return;
                    var ManaRH = Menu.Item("RManaH").GetValue<Slider>().Value;
                    if (getUnitsInPath(Player, RTarget, R) && getPerValue(true) >= ManaRH && !Player.HasBuff("dravenrdoublecast", true))
                    {
                        R.Cast(RTarget, isMenuEnabled("Packets"));
                    }
                    break;
            }
        }

        private static void RExecute(Obj_AI_Hero RTarget)
        {
            var Pred = R.GetPrediction(RTarget);
            if (!RTarget.IsValidTarget() || Pred.Hitchance < HitChance.Medium || !R.IsReady()) return;
            if (getUnitsInPath(Player, RTarget, R) &&  !Player.HasBuff("dravenrdoublecast", true))
            {
                R.Cast(RTarget, isMenuEnabled("Packets"));
            }
        }
        static void castItems(Obj_AI_Hero tar)
        {
            var ownH = getPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }
            if ((Menu.Item("BotrkH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
               ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }
            if (Menu.Item("YoumuuC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
            {
                UseItem(3142);
            }
            if (Menu.Item("YoumuuH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
            {
                UseItem(3142);
            }
            if (Menu.Item("BilgeC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
            {
                UseItem(3144, tar);
            }
            if (Menu.Item("BilgeH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
            {
                UseItem(3144, tar);
            }
        }
        private static bool hasWBuff()
        {
            //dravenfurybuff
            //DravenFury
            return Player.HasBuff("DravenFury", true) || Player.HasBuff("dravenfurybuff",true);
        }
        public static bool minionThere()
        {
            var List = MinionManager.GetMinions(Player.Position, xSLxOrbwalker.GetAutoAttackRange())
                .Where(m => HealthPrediction.GetHealthPrediction(m,
                    (int) (Player.Distance(m)/Orbwalking.GetMyProjectileSpeed())*1000) <=
                            Q.GetDamage(m) + Player.GetAutoAttackDamage(m)
                        ).ToList();
           // Game.PrintChat("QDmg "+Q.GetDamage(List.FirstOrDefault()));
            return  List.Count > 0;
        }
        public static Vector3 PosAfterRange(Vector3 p1, Vector3 finalp2, float range)
        {
            var Pos2 = Vector3.Normalize(finalp2 - p1);
            return p1 + (Pos2 * range);
        }
        public static int GetQStacks()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
            return buff != null ? buff.Count : 0;
        }

        #region Utility Methods
        public static bool isMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }
        static float getPerValue(bool mana)
        {
            if (mana) return (Player.Mana / Player.MaxMana) * 100;
            return (Player.Health / Player.MaxHealth) * 100;
        }
        static float getPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana) return (target.Mana / target.MaxMana) * 100;
            return (target.Health / target.MaxHealth) * 100;
        }
        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        public static bool isUnderEnTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        private static bool getUnitsInPath(Obj_AI_Hero player, Obj_AI_Hero target, Spell spell)
        {
            float distance = player.Distance(target);
            List<Obj_AI_Base> minionList = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spell.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            int numberOfMinions = (from Obj_AI_Minion minion in minionList
                                   let skillshotPosition =
                                       V2E(player.Position,
                                           V2E(player.Position, target.Position,
                                               Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                                           Vector3.Distance(player.Position, minion.Position))
                                   where skillshotPosition.Distance(minion) < spell.Width
                                   select minion).Count();
            int numberOfChamps = (from minion in ObjectManager.Get<Obj_AI_Hero>()
                                  let skillshotPosition =
                                      V2E(player.Position,
                                          V2E(player.Position, target.Position,
                                              Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                                          Vector3.Distance(player.Position, minion.Position))
                                  where skillshotPosition.Distance(minion) < spell.Width && minion.IsEnemy
                                  select minion).Count();
            int totalUnits = numberOfChamps + numberOfMinions - 1;
            // total number of champions and minions the projectile will pass through.
            if (totalUnits == -1) return false;
            double damageReduction = 0;
            damageReduction = ((totalUnits > 7)) ? 0.4 : (totalUnits == 0) ? 1.0 : (1 - ((totalUnits) / 12.5));
            // the damage reduction calculations minus percentage for each unit it passes through!
            return spell.GetDamage(target) * damageReduction >= (target.Health + (distance / 2000) * target.HPRegenRate);
            // - 15 is a safeguard for certain kill.
        }
        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }
        #endregion
    }

    internal class PossibleReticle
    {
        public GameObject AxeGameObject;
        public int networkID;
        public Vector3 Position;
        public double CreationTime;
        public double EndTime;

        public PossibleReticle(GameObject Axe)
        {
            AxeGameObject = Axe;
            networkID = Axe.NetworkId;
            Position = Axe.Position;
            CreationTime = Game.Time;
            EndTime = Game.Time + 1.20;
        }

        public bool canCatch(bool UseW,out bool ShouldUseW)
        {
            var EnemyHeroesCount =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsEnemy && h.IsValidTarget() && h.Distance(Position) <= DZDraven_Reloaded.Menu.Item("SafeZone").GetValue<Slider>().Value).ToList();
            if ((DZDraven_Reloaded.isUnderEnTurret(Position) && !DZDraven_Reloaded.isUnderEnTurret(ObjectManager.Player.Position)) || EnemyHeroesCount.Count > 0)
            {
                ShouldUseW = false;
                return false;
            }
            var distance = ObjectManager.Player.GetPath(Position).ToList().To2D().PathLength();
            var catchNormal = distance / ObjectManager.Player.MoveSpeed + Game.Time < EndTime; // Not buffed with W, Normal
            var AdditionalSpeed = (5*DZDraven_Reloaded.W.Level + 35)*0.01*ObjectManager.Player.MoveSpeed;
            var catchBuff = distance / (ObjectManager.Player.MoveSpeed +  AdditionalSpeed + Game.Time) < EndTime; //Buffed with W
            if (catchNormal)
            {
                ShouldUseW = false;
                return catchNormal;
            }
            if (UseW && !catchNormal && catchBuff)
            {
                ShouldUseW = true;
                return catchBuff;
            }
            ShouldUseW = false;
            return false;
        }
        public float Distance()
        {
            return Vector3.Distance(Position, ObjectManager.Player.Position);
        }

        public bool isCatchingNow()
        {
            return Distance() < 49 + (ObjectManager.Player.BoundingRadius/2) + 50; //Taken from PUC Draven
        }
    }
}
