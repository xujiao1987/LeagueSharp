using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace NasusFeelTheCane
{
    class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Obj_AI_Hero Player;
        public static Int32 Sheen = 3057, Iceborn = 3025;

        public static List<NewBuff> buffList =  new List<NewBuff>
        {
            
            new NewBuff()
            {
                DisplayName = "PantheonPassiveShield", Name = "pantheonpassiveshield"
            },
            new NewBuff()
            {
                DisplayName = "FioraRiposte", Name = "FioraRiposte"
            },
        };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            var jungleMinions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All,
                    MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var laneMinions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth);

            if (Config.Item("AutoLastHitQ").GetValue<KeyBind>().Active && !Player.HasBuff("Recall") && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                foreach (var minion in laneMinions)
                {
                    if (GetBonusDmg(minion) > minion.Health &&
                        minion.Distance(Player) < Orbwalking.GetRealAutoAttackRange(Player) + 50 && Q.IsReady())
                    {
                        Orbwalker.SetAttack(false);
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        Orbwalker.SetAttack(true);
                        break;
                    }
                }
            }
            
            Obj_AI_Hero target = SimpleTs.GetTarget(800, SimpleTs.DamageType.Physical);
            if ((Player.Health/Player.MaxHealth*100) <= Config.Item("minRHP").GetValue<Slider>().Value && !Utility.InFountain())
            {
                if ((Config.Item("minRChamps").GetValue<Slider>().Value == 0) ||
                    (Config.Item("minRChamps").GetValue<Slider>().Value > 0) &&
                    Utility.CountEnemysInRange(800) >= Config.Item("minRChamps").GetValue<Slider>().Value)
                {
                    R.Cast(true);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target != null)
            {
                if (target.IsValidTarget(W.Range) && paramBool("ComboW")) W.CastOnUnit(target);
                if (target.IsValidTarget(E.Range + E.Width) && paramBool("ComboE")) E.Cast(target, Config.Item("packets").GetValue<bool>());
                if (hasAntiAA(target)) return;
                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 100) && paramBool("ComboQ"))
                {
                    Q.Cast(Config.Item("packets").GetValue<bool>());
                }
                
            }
            if (isFarmMode())
            {
              
                if((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
                {
                    if (jungleMinions.Count > 0)
                    {
                        if (Q.IsReady() && Q.IsReady() && paramBool("WaveClearQ"))
                        {
                            Q.Cast(Config.Item("packets").GetValue<bool>());
                        }
                        if (!E.IsReady() && paramBool("WaveClearE"))
                        {
                            List<Vector2> minionerinos2 =
                                (from minions in jungleMinions select minions.Position.To2D()).ToList();
                            var ePos2 =
                                MinionManager.GetBestCircularFarmLocation(minionerinos2, E.Width, E.Range).Position;
                            if (ePos2.Distance(Player.Position.To2D()) < E.Range)
                            {
                                E.Cast(ePos2, Config.Item("packets").GetValue<bool>());
                            }
                        }
                    }

                    if (jungleMinions.Count > 0) return;
                    foreach (var minion in laneMinions)
                    {
                        if (GetBonusDmg(minion) > minion.Health &&
                            minion.Distance(Player) < Orbwalking.GetRealAutoAttackRange(Player) + 50 && Q.IsReady() && paramBool("JungleQ"))
                        {
                            Orbwalker.SetAttack(false);
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            Orbwalker.SetAttack(true);
                            break;
                        }
                    }
                    if (!E.IsReady() && paramBool("JungleE"))
                    {
                        List<Vector2> minionerinos =
                            (from minions in laneMinions select minions.Position.To2D()).ToList();
                        var ePos2 =
                            MinionManager.GetBestCircularFarmLocation(minionerinos, E.Width, E.Range).Position;
                        if (ePos2.Distance(Player.Position.To2D()) < E.Range)
                        {
                            E.Cast(ePos2, Config.Item("packets").GetValue<bool>());
                        }
                    }
                }
                if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit) && paramBool("LastHitQ"))
                {
                    if (jungleMinions.Count > 0) return;
                    foreach (var minion in laneMinions)
                    {
                        if (GetBonusDmg(minion) > minion.Health &&
                            minion.Distance(Player) < Orbwalking.GetRealAutoAttackRange(Player) + 50 && Q.IsReady())
                        {
                            Orbwalker.SetAttack(false);
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            Orbwalker.SetAttack(true);
                            break;
                        }
                    }
                }
            }
        }

        public static bool hasAntiAA(Obj_AI_Hero target)
        {
            foreach (var buff in buffList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name) ||
                    Player.HasBuffOfType(BuffType.Blind)) return true;
            }
            return false;
        } 

        public static bool isFarmMode()
        {  
            return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear ||
                   Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit;
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().Contains("attack") || !sender.IsMe) return;
            var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.Target.NetworkId);
            if ((GetBonusDmg(unit) > unit.Health))
            {
                Q.Cast(Config.Item("packets").GetValue<bool>());
            }
        }

        // From Master of Nasus + modified by me
        private static double GetBonusDmg(Obj_AI_Base target)
        {
            double DmgItem = 0;
            if (Items.HasItem(Sheen) && (Items.CanUseItem(Sheen) || Player.HasBuff("sheen", true)) && Player.BaseAttackDamage > DmgItem) DmgItem = Damage.GetAutoAttackDamage(Player, target);
            if (Items.HasItem(Iceborn) && (Items.CanUseItem(Iceborn) || Player.HasBuff("itemfrozenfist", true)) && Player.BaseAttackDamage * 1.25 > DmgItem) DmgItem = Damage.GetAutoAttackDamage(Player, target) * 1.25;
            return Q.GetDamage(target) + Damage.GetAutoAttackDamage(Player, target) + DmgItem;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, Orbwalking.GetRealAutoAttackRange(Player));
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 0);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            Config = new Menu("沙漠死神", "nftc", true);

            var OWMenu = Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(OWMenu);
            var TSMenu = Config.AddSubMenu(new Menu("目标选择", "Target Selector"));
            SimpleTs.AddToMenu(TSMenu);
            var ComboMenu = Config.AddSubMenu(new Menu("连招", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboQ", "使用Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboW", "使用W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboE", "使用E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ndskafjk", "-- R设置"));
            ComboMenu.AddItem(new MenuItem("ComboR", "使用R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("minRHP", "最小血量").SetValue(new Slider(1, 1)));
            ComboMenu.AddItem(new MenuItem("minRChamps", "最少敌方英雄").SetValue(new Slider(0, 0, 5)));
            ComboMenu.AddItem(new MenuItem("fsffs", "设置0禁用"));

            var FarmMenu = Config.AddSubMenu(new Menu("补兵", "Farm"));
            FarmMenu.AddItem(new MenuItem("AutoLastHitQ", "自动Q补兵").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            FarmMenu.AddItem(new MenuItem("pratum", "-- 补兵"));
            FarmMenu.AddItem(new MenuItem("LastHitQ", "使用Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("pratum2", "-- 清线"));
            FarmMenu.AddItem(new MenuItem("WaveClearQ", "使用Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("WaveClearE", "使用E").SetValue(true));
            FarmMenu.AddItem(new MenuItem("pratum22", "-- 清野"));
            FarmMenu.AddItem(new MenuItem("JungleQ", "使用Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("JungleE", "使用E").SetValue(true));

            var DrawMenu = Config.AddSubMenu(new Menu("血量指示器", "HP Bar Indicator"));
            DrawMenu.AddItem(new MenuItem("drawAA", "显示平A血格").SetValue(false));
            DrawMenu.AddItem(new MenuItem("LineAAThicknessColour", "厚度/颜色").SetValue(new Circle(true, Color.CornflowerBlue, 10)));
            DrawMenu.AddItem(new MenuItem("drawHPBar", "显示平A/Q/物品").SetValue(true));
            DrawMenu.AddItem(new MenuItem("LineThicknessColour", "厚度/颜色").SetValue(new Circle(true, Color.White, 10)));


            Config.AddItem(new MenuItem("packets", "封包")).SetValue(true);

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            List<Obj_AI_Base> minionList = MinionManager.GetMinions(Player.Position,
                Orbwalking.GetRealAutoAttackRange(Player) + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).ToList();
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 500)))
            {
                var attackToKill = Math.Ceiling(minion.MaxHealth / GetBonusDmg(minion));
                var hpBarPosition = minion.HPBarPosition;
                var barWidth = minion.IsMelee() ? 75 : 80;
                if (minion.HasBuff("turretshield", true))
                    barWidth = 70;

                var barDistance = (float)(barWidth / attackToKill);
                if (Config.Item("drawHPBar").GetValue<bool>())
                {
                        var startposition = hpBarPosition.X + 45 + barDistance;
                        Drawing.DrawLine(
                            new Vector2(startposition, hpBarPosition.Y + 18),
                            new Vector2(startposition, hpBarPosition.Y + 23),
                            2,
                            Config.Item("LineThicknessColour").GetValue<Circle>().Color);
                }
                if (Config.Item("drawAA").GetValue<bool>())
                {
                   attackToKill =  Math.Ceiling(minion.MaxHealth / Player.GetAutoAttackDamage(minion));
                   barDistance = (float)(barWidth / attackToKill);
                   var startposition = hpBarPosition.X + 45 + barDistance;
                   Drawing.DrawLine(
                       new Vector2(startposition, hpBarPosition.Y + 18),
                       new Vector2(startposition, hpBarPosition.Y + 23),
                       2,
                       Config.Item("LineAAThicknessColour").GetValue<Circle>().Color);
                }
            } 
        }

        public static bool paramBool(String menuName)
        {
            return Config.Item(menuName).GetValue<bool>();
        }
    }
}
