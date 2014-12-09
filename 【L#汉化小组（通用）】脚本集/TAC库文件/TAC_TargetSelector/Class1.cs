using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace TAC_TargetSelector
{
    public class TS
    {
        private static Menu _Config;
        public static TargetingMode _Mode;
        public static Obj_AI_Hero Target;
        private static Dictionary<string,float> targetsPriority = new Dictionary<string,float>();
        public enum DamageType
        {
            Magical,
            Physical,
            True,
        }
        #region Targets

        private static readonly string[] ap =
        {
            "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana",
            "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen",
            "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion",
            "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra",
            "Velkoz"
        };
        private static readonly string[] sup =
        {
            "Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona",
            "Soraka", "Thresh", "Zilean"
        };
        private static readonly string[] tank =
        {
            "Amumu", "Chogath", "DrMundo", "Galio", "Hecarim", "Malphite",
            "Maokai", "Nasus", "Rammus", "Sejuani", "Shen", "Singed", "Skarner", "Volibear", "Warwick", "Yorick", "Zac",
            "Nunu", "Taric", "Alistar", "Garen", "Nautilus", "Braum"
        };
        private static readonly string[] ad =
        {
            "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "KogMaw",
            "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx",
            "Yasuo", "Lucian", "Kalista"
        };
        private static readonly string[] bruiser =
        {
            "Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce",
            "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton",
            "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox",
            "Rumble", "Shaco", "MasterYi"
        };
        #endregion
        public enum TargetingMode
        {
            LowHP,
            MostAD,
            MostAP,
            Closest,
            NearMouse,
            AutoPriority,
            LessAttack,
            LessCast,
            Automatic,
        }
        static TS()
        {
            Drawing.OnDraw += onDraw;
            Game.OnWndProc += selectTarget;
        }
        private static void onDraw(EventArgs args)
        {
            if (Target.IsValidTarget() && _Config != null && _Config.Item("FocusSelected").GetValue<bool>() &&
               _Config.Item("SelTColor").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Target.Position, 150, _Config.Item("SelTColor").GetValue<Circle>().Color, 7, true);
            }
        }
        public static Obj_AI_Hero getTarget(DamageType damageType,float range = 600)
        {
            Obj_AI_Hero newtarget = null;
            if (Target.IsValidTarget() && !IsInvulnerable(Target) && (range < 0 && Orbwalking.InAutoAttackRange(Target) || ObjectManager.Player.Distance(Target) < range))
            {
                return Target;
            }
            if (_Mode != TargetingMode.AutoPriority && _Mode != TargetingMode.Automatic)
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget() && ObjectManager.Player.Distance(target) <= range))
                {
                    switch (_Mode)
                    {
                        case TargetingMode.LowHP:
                            if (target.Health < newtarget.Health) newtarget = target;
                            break;
                        case TargetingMode.MostAD:
                            if (target.BaseAttackDamage + target.FlatPhysicalDamageMod < newtarget.BaseAttackDamage + newtarget.FlatPhysicalDamageMod)
                                newtarget = target;
                            break;
                        case TargetingMode.MostAP:
                            if (target.FlatMagicDamageMod < newtarget.FlatMagicDamageMod)
                                newtarget = target;
                            break;
                        case TargetingMode.Closest:
                            if (ObjectManager.Player.Distance(target) < ObjectManager.Player.Distance(newtarget))
                                newtarget = target;
                            break;
                        case TargetingMode.NearMouse:
                            if (SharpDX.Vector2.Distance(Game.CursorPos.To2D(), target.Position.To2D()) + 50 <
                            SharpDX.Vector2.Distance(Game.CursorPos.To2D(), newtarget.Position.To2D()))
                                newtarget = target;
                            break;
                        case TargetingMode.LessAttack:
                            if ((target.Health -
                            ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, target.Health) <
                            (newtarget.Health -
                            ObjectManager.Player.CalcDamage(
                            newtarget, Damage.DamageType.Physical, newtarget.Health))))
                                newtarget = target;
                            break;
                        case TargetingMode.LessCast:
                            if ((target.Health -
                            ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, target.Health) <
                            (newtarget.Health -
                            ObjectManager.Player.CalcDamage(
                            newtarget, Damage.DamageType.Magical, newtarget.Health))))
                                newtarget = target;
                            break;
                    }
                }
            }
            else if (_Mode != TargetingMode.Automatic && _Mode == TargetingMode.AutoPriority)
            {
                int prio = 5;

                foreach (var target in
                ObjectManager.Get<Obj_AI_Hero>()
                .Where(target => target != null && target.IsValidTarget() && Geometry.Distance(target) <= range))
                {
                    var priority = FindPrioForTarget(target.ChampionName);
                    if (newtarget == null)
                    {
                        newtarget = target;
                        prio = priority;
                    }
                    else
                    {
                        if (priority < prio)
                        {
                            newtarget = target;
                            prio = FindPrioForTarget(target.ChampionName);
                        }
                        else if (priority == prio)
                        {
                            if (!(target.Health < newtarget.Health))
                            {
                                continue;
                            }
                            newtarget = target;
                            prio = priority;
                        }
                    }
                }
            }
            else
            {
                var bestRatio = 0f;
//                targetsPriority.Clear(); // Refresh list
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (!hero.IsValidTarget() || IsInvulnerable(hero) ||
                        ((!(range < 0) || !Orbwalking.InAutoAttackRange(hero)) && !(ObjectManager.Player.Distance(hero) < range)))
                    {
                        continue;
                    }
                    var damage = 0f;

                    switch (damageType)
                    {
                        case DamageType.Magical:
                            damage = (float)ObjectManager.Player.CalcDamage(hero, Damage.DamageType.Magical, 100);
                            break;
                        case DamageType.Physical:
                            damage = (float)ObjectManager.Player.CalcDamage(hero, Damage.DamageType.Physical, 100);
                            break;
                        case DamageType.True:
                            damage = 100;
                            break;
                    }



                    var ratio = damage / (1 + hero.Health) * GetPriority(hero);

//                    targetsPriority.Add(hero.BaseSkinName, ratio);
                    if (ratio > bestRatio)
                    {
                        bestRatio = ratio;
                        newtarget = hero;
                    }
                }
                // after we done some calculations, we should select the new target
//                KeyValuePair<string,float> bestTarget = targetsPriority.OrderByDescending(kv => kv.Value).Last();
//                Game.PrintChat("Best target is: "+bestTarget);
            }
            return newtarget;
        }
        public static float GetPriority(Obj_AI_Hero hero)
        {
            var p = 1;
            if (_Config != null && _Config.Item("priority"+hero.ChampionName) != null)
            {
                p = _Config.Item("priority" + hero.ChampionName).GetValue<Slider>().Value;
            }

            switch (p)
            {
                case 1:
                default:
                    return 2.5f;
                case 2:
                    return 2f;
                case 3:
                    return 1.75f;
                case 4:
                    return 1.5f;
                case 5:
                    return 1f;
            }
        }
        public static bool IsInvulnerable(Obj_AI_Base target)
        {
            //TODO: add yasuo wall, spellshields, etc.
            if (target.HasBuff("Undying Rage") && target.Health >= 2f)
            {
                return true;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }

            return false;
        }
        private static int FindPrioForTarget(string ChampionName)
        {
            return ap.Contains(ChampionName) ? 2 : (ad.Contains(ChampionName) ? 1 : (sup.Contains(ChampionName) ? 4 : (bruiser.Contains(ChampionName) ? 3 : 5)));
        }
        public static void createMenu(Menu Config)
        {
            _Config = Config;
            Config.AddSubMenu(new Menu("目标选择","tac_targetSelector"));
            Config.SubMenu("tac_targetSelector").AddSubMenu(new Menu("目标","targets"));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                Config.SubMenu("tac_targetSelector").SubMenu("targets").AddItem(new MenuItem("priority" + enemy.ChampionName, enemy.ChampionName).SetShared().SetValue(new Slider(FindPrioForTarget(enemy.ChampionName), 5, 1)));
            }
            Config.SubMenu("tac_targetSelector").SubMenu("targets").AddItem(new MenuItem("autoArrange","自动排列").SetValue(false));
            Config.SubMenu("tac_targetSelector").SubMenu("targets").Item("autoArrange").ValueChanged += prioritizeChampion;


            Config.SubMenu("tac_targetSelector").AddItem(new MenuItem("currentMode", "模式").SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)), 2)));
            Config.SubMenu("tac_targetSelector").Item("currentMode").ValueChanged += currentMode;

            Config.SubMenu("tac_targetSelector").AddItem(new MenuItem("FocusSelected", "锁定目标").SetShared().SetValue(true));
            Config.SubMenu("tac_targetSelector").AddItem(
                new MenuItem("SelTColor", "颜色").SetShared()
                    .SetValue(new Circle(true, System.Drawing.Color.Red)));
        }
        public static TargetingMode getCurrentMode()
        {
            return _Mode;
        }
        private static void currentMode(object sender, OnValueChangeEventArgs e)
        {
            switch(e.GetNewValue<StringList>().SelectedIndex)
            {
                case 0:
                default:
                    _Mode = TargetingMode.LowHP;
                    break;
                case 1:
                    _Mode = TargetingMode.MostAD;
                    break;
                case 2:
                    _Mode = TargetingMode.MostAP;
                    break;
                case 3:
                    _Mode = TargetingMode.Closest;
                    break;
                case 4:
                    _Mode = TargetingMode.NearMouse;
                    break;
                case 5:
                    _Mode = TargetingMode.AutoPriority;
                    break;
                case 6:
                    _Mode = TargetingMode.LessAttack;
                    break;
                case 7:
                    _Mode = TargetingMode.LessCast;
                    break;
            }
        }
        private static void prioritizeChampion(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>()) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != ObjectManager.Player.Team))
            {
                _Config.Item("priority" + enemy.ChampionName).SetValue(new Slider(FindPrioForTarget(enemy.ChampionName), 5, 1));
            }
        }
        private static void selectTarget(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN) return;
            Target = null;
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget())
                    .OrderByDescending(h => h.Distance(Game.CursorPos))
                    .Where(enemy => enemy.Distance(Game.CursorPos) < 200))
            {
                Target = enemy;
            }
        }
    }
}