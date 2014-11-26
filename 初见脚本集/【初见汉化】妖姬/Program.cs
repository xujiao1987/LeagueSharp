#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace FedLeblanc
{
    internal class Program
    {
        public const string ChampionName = "Leblanc";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static SpellSlot IgniteSlot;
        public static Items.Item DFG;

        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 700);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            W.SetSkillshot(0.5f, 220, 1300, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 95, 1600, true, SkillshotType.SkillshotLine);            

            SpellList.AddRange(new[] { Q, W, E, R });

            Config = new Menu("|鍒濊姹夊寲-濡栧К|" + ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("|鍒濊姹夊寲-鐩爣閫夋嫨|", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-璧扮爫|", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-杩炴嫑|", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "閫夐」").SetValue(new StringList(new[] { "Q+R+W+E", "Q+W+R+E" }, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseDFGCombo", "浣跨敤鍐ョ伀").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("BackCombo", "杩斿洖W鍦ㄤ綆HP/MP鎴栧噺閫熴€€").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "浣跨敤鐐圭噧").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "杩炴嫑!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-楠氭壈|", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤 W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤 E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "浣跨敤 R").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWQHarass", "浣跨敤W+Q瓒呭嚭鑼冨洿").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("BackHarass", "W杩斿洖").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassToggleQ", "浣跨敤 Q (鍒囨崲)").SetValue<KeyBind>(new KeyBind('T', KeyBindType.Toggle)));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "楠氭壈!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-娓呯嚎|", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤 Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "浣跨敤 W").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "浣跨敤 E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));
            Config.SubMenu("Farm").AddItem(new MenuItem("ManaFarm", "鏈€灏忕殑娉曞姏").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("waveNumW", "澶氬皯灏忓叺浣跨敤W").SetValue<Slider>(new Slider(4, 1, 10)));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "鍐荤粨!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "娓呯嚎!").SetValue(new KeyBind("V".ToCharArray()[0],KeyBindType.Press)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-娓呴噹|", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "浣跨敤 W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "浣跨敤 E").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "娓呴噹!").SetValue(new KeyBind("V".ToCharArray()[0],KeyBindType.Press)));            

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "鎹熶激鍚庢棆杞€€").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-鎶€鑳借寖鍥撮€夐」|", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 浣跨敤").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 浣跨敤").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 浣跨敤").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WQRange", "WQ 浣跨敤").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("UsePacket","浣跨敤灏佸寘").SetValue(true));
            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-缇ゅ彿|", "by chujian"));
            Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
            Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao1", "浜ゆ祦缇わ細333399"));
            Game.OnGameUpdate += Game_OnGameUpdate;           
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
           
            Game.PrintChat("<font color=\"#00BFFF\">鍒濊姹夊寲" + ChampionName + " -</font> <font color=\"#FFFFFF\">鍔犺浇鎴愬姛! QQ 5011477</font>");            
        } 

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {            
            PredictionOutput ePred = E.GetPrediction(gapcloser.Sender);
            if (ePred.Hitchance >= HitChance.High)
                E.Cast(ePred.CastPosition);
        }
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            PredictionOutput ePred = E.GetPrediction(unit);
            if (ePred.Hitchance >= HitChance.High)
                E.Cast(ePred.CastPosition);            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            var menuItem = Config.Item("WQRange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, W.Range+Q.Range, menuItem.Color);
            
            foreach (var spell in SpellList)
            {
                menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {                
                Combo();                
            }
            else
            { 
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (Config.Item("harassToggleQ").GetValue<KeyBind>().Active)
                    ToggleHarass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
                                   
        } 

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private static void Combo()
        {
            var ComboMode = Config.Item("ComboMode").GetValue<StringList>().SelectedIndex;

            switch (ComboMode)
            {
                case 0:
                    Combo1();
                    break;
                case 1:
                    Combo2();
                    break;
            }                
        }

        private static void Combo1() // Q+R+W+E
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var comboDamage = wTarget != null ? GetComboDamage(wTarget) : 0;

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var userDFG = Config.Item("UseDFGCombo").GetValue<bool>();
            var UseIgniteCombo = Config.Item("UseIgniteCombo").GetValue<bool>();

            if (Q.IsReady() && R.IsReady() && useQ && useR)
            {
                if (qTarget != null)
                {
                    Q.CastOnUnit(qTarget);

                    if (userDFG && wTarget != null && comboDamage > wTarget.Health && DFG.IsReady() && W.IsReady() && R.IsReady())
                    {
                        DFG.Cast(wTarget);
                    }

                    if (Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancChaos"))
                    {
                        R.CastOnUnit(qTarget);
                    }
                }
            }
            else
            {
                if (useW && wTarget != null && W.IsReady() && !LeblancPulo())
                {
                    W.CastOnUnit(wTarget);
                }

                if (useE && eTarget != null && E.IsReady())
                {
                    PredictionOutput ePred = E.GetPrediction(eTarget);
                    if (ePred.Hitchance >= HitChance.High)
                        E.Cast(ePred.CastPosition);
                }

                if (useQ && qTarget != null && Q.IsReady())
                {
                    Q.CastOnUnit(qTarget);
                }

                if (useR && qTarget != null && R.IsReady() && Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancChaos"))
                {
                    R.Cast(qTarget);
                }

                if (Config.Item("BackCombo").GetValue<bool>() && LeblancPulo() && (qTarget == null || 
                    !W.IsReady() && !Q.IsReady() && !R.IsReady() || 
                    GetHPPercent() < 15 ||
                    GetMPPercent() < 15 ))
                {
                    W.Cast();
                }
            }

            if (wTarget != null && IgniteSlot != SpellSlot.Unknown &&
                        Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (Player.Distance(wTarget) < 650 && comboDamage > wTarget.Health && UseIgniteCombo)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, wTarget);
                }
            }            
        }

        private static void Combo2() // Q+W+R+E
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var comboDamage = wTarget != null ? GetComboDamage(wTarget) : 0;

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var userDFG = Config.Item("UseDFGCombo").GetValue<bool>();
            var UseIgniteCombo = Config.Item("UseIgniteCombo").GetValue<bool>();

            if (Q.IsReady() && W.IsReady() && R.IsReady() && useQ && useR && useW)
            {
                if (qTarget != null)
                {
                    Q.CastOnUnit(qTarget);

                    if (userDFG && wTarget != null && comboDamage > wTarget.Health && DFG.IsReady() && W.IsReady() && R.IsReady())
                    {
                        DFG.Cast(wTarget);
                    }

                    if (Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancChaos"))
                    {
                        W.CastOnUnit(qTarget);
                    }

                    if (Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancSlideM") && LeblancPulo())
                    {
                        R.CastOnUnit(qTarget);
                    }
                    
                }
            }
            else
            {
                if (useE && eTarget != null && E.IsReady())
                {
                    PredictionOutput ePred = E.GetPrediction(eTarget);
                    if (ePred.Hitchance >= HitChance.High)
                        E.Cast(ePred.CastPosition);
                }

                if (useW && wTarget != null && W.IsReady() && !LeblancPulo())
                {
                    W.CastOnUnit(wTarget);
                }

                if (useQ && qTarget != null && Q.IsReady())
                {
                    Q.CastOnUnit(qTarget);
                }

                if (useR && qTarget != null && R.IsReady() && (Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancChaos") ||
                                                               Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancSlideM")))
                {
                    R.Cast(qTarget);
                }

                if (Config.Item("BackCombo").GetValue<bool>() && LeblancPulo() && (qTarget == null ||
                    !W.IsReady() && !Q.IsReady() && !R.IsReady() ||
                    GetHPPercent() < 15 ||
                    GetMPPercent() < 15))
                {
                    W.Cast();
                }
            }

            if (wTarget != null && IgniteSlot != SpellSlot.Unknown &&
                        Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (Player.Distance(wTarget) < 650 && comboDamage > wTarget.Health && UseIgniteCombo)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, wTarget);
                }
            }
        }

        private static float GetHPPercent()
        {
            return (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100f;
        }
        private static float GetMPPercent()
        {
            return (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100f;
        }

        private static void ToggleHarass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (target != null && Q.IsReady())
            {
                Q.CastOnUnit(target);
            }

        }

        private static bool LeblancPulo()
        {
            if (!W.IsReady() || Player.Spellbook.GetSpell(SpellSlot.W).Name == "leblancslidereturn")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var rtarget = SimpleTs.GetTarget(W.Range + Q.Range, SimpleTs.DamageType.Magical);            
            
            if (Config.Item("UseWQHarass").GetValue<bool>() && Player.Distance(rtarget) > Q.Range && Player.Distance(rtarget) <= W.Range + Q.Range)
            {
                if (W.IsReady() && Q.IsReady())
                {
                    W.Cast(rtarget.ServerPosition); 
                }

                if (target != null && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady() && Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancSlideM"))
                {
                    Q.CastOnUnit(target);
                }
                
            }
            else
            {
                if (target != null && Config.Item("UseRHarass").GetValue<bool>() && R.IsReady() && Player.Spellbook.GetSpell(SpellSlot.R).Name.Contains("LeblancChaos"))
                {
                    R.CastOnUnit(target);
                }

                if (target != null && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
                {
                    Q.CastOnUnit(target);
                }

                if (target != null && Config.Item("UseWHarass").GetValue<bool>() && !LeblancPulo())
                {
                    W.CastOnUnit(target);
                }

                if (target != null && Config.Item("UseEHarass").GetValue<bool>())
                {
                    PredictionOutput ePred = E.GetPrediction(target);
                    if (ePred.Hitchance >= HitChance.High)
                        E.Cast(ePred.CastPosition);
                }
                
                if (Config.Item("UseWQHarass").GetValue<bool>() && Config.Item("BackHarass").GetValue<bool>() && LeblancPulo() && !Q.IsReady())
                {
                    if (Config.Item("UseRHarass").GetValue<bool>() && R.IsReady()) return;
                    if (Config.Item("UseEHarass").GetValue<bool>() && E.IsReady()) return;

                    W.Cast();
                }
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.Ranged);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.All);

            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.All);            

            var FMana = Config.Item("ManaFarm").GetValue<Slider>().Value;
            var MPercent = Player.Mana * 100 / Player.MaxMana;

            // Spell usage
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && MPercent >= FMana && (useQi == 1 || useQi == 2)) || (!laneClear && MPercent >= FMana && (useQi == 0 || useQi == 2));
            var useW = (laneClear && MPercent >= FMana && (useWi == 1 || useWi == 2)) || (!laneClear && MPercent >= FMana && (useWi == 0 || useWi == 2));
            var useE = (laneClear && MPercent >= FMana && (useEi == 1 || useEi == 2)) || (!laneClear && MPercent >= FMana && (useEi == 0 || useEi == 2));

            var fl1 = W.GetCircularFarmLocation(rangedMinionsW, W.Width);
            var fl2 = W.GetCircularFarmLocation(allMinionsW, W.Width);

            if (useQ)
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        minion.Health < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);                        
                    }
                }
            }
            else if (useW)
            {     
                if (fl1.MinionsHit >= Config.Item("waveNumW").GetValue<Slider>().Value && fl1.MinionsHit >= 3)
                {
                    W.Cast(fl1.Position);
                }                
            }
            else if (useE)
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        minion.Health < 0.80 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.Cast(minion);                        
                    }
                }
            }
            if (laneClear)
            {
                foreach (var minion in allMinions)
                {
                    if (useQ && minion.IsValidTarget() &&
                        minion.Health < 0.80 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                        Q.CastOnUnit(minion);

                    if (useW)                        

                if (fl1.MinionsHit >= Config.Item("waveNumW").GetValue<Slider>().Value && fl1.MinionsHit >= 3)
                {
                    W.Cast(fl1.Position);
                }             

                    if (useE)
                        E.Cast(minion);
                }
            }
        }

        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                Q.Cast(mob);
                W.Cast(mob);                
                E.Cast(mob);                
            }
        }       
    }
}