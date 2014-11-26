using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;

namespace AlienHack_XinSharp
{
    internal class Program
    {
        public static Menu Config;

        public static Spell QSpell;
        public static Spell WSpell;
        public static Spell ESpell;
        public static Spell RSpell;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static SpellDataInst smiteSlot;
        public static SpellSlot igniteSlot;
        public static Int32 lastSkinId = 0;
        public static Items.Item tiamat, hydra, blade, bilge, rand, lotis;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "XinZhao") return;

            //Spells
            QSpell = new Spell(SpellSlot.Q, 375);
            WSpell = new Spell(SpellSlot.W, 20);
            ESpell = new Spell(SpellSlot.E, 650);
            RSpell = new Spell(SpellSlot.R, 500);
            smiteSlot = Player.SummonerSpellbook.GetSpell(Player.GetSpellSlot("summonersmite"));
            igniteSlot = Player.GetSpellSlot("SummonerDot");

            bilge = new Items.Item(3144, 475f);
            blade = new Items.Item(3153, 425f);
            hydra = new Items.Item(3074, 375f);
            tiamat = new Items.Item(3077, 375f);
            rand = new Items.Item(3143, 490f);
            lotis = new Items.Item(3190, 590f);


            //Make the menu
            Config = new Menu("XinSharp", "The Dragon Talon", true);



            //Orbwalker submenu            


            //Lxorbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "LX_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);

            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R Kill Secured").SetValue(true));

            //LaneClear menu
            Config.AddSubMenu(new Menu("LaneClear/Jungle Clear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "Use W").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("useEKS", "Use E KS").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("useRIn", "Use R Interrupt").SetValue(true));


            //Items public static Int32 Tiamat = 3077, Hydra = 3074, Blade = 3153, Bilge = 3144, Rand = 3143, lotis = 3190;
            Config.AddSubMenu(new Menu("items", "items"));
            Config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));
              

            //SummonerSpell Menu
            Config.AddSubMenu(new Menu("SummonerSpell", "SummonerSpell"));
            Config.SubMenu("SummonerSpell").AddItem(new MenuItem("usesmite", "Use Smite(Toggle)").SetValue(new KeyBind("N".ToCharArray()[0],
        KeyBindType.Toggle)));
            Config.SubMenu("SummonerSpell").AddItem(new MenuItem("useIgnite", "use Ignite KS").SetValue(true));
           
            //Skin Changer
            Config.AddSubMenu(new Menu("Skin Changer", "SkinChanger"));
            Config.SubMenu("SkinChanger").AddItem(new MenuItem("skin", "Use Custom Skin").SetValue(true));
            Config.SubMenu("SkinChanger").AddItem(new MenuItem("skin1", "Skin Changer").SetValue(new Slider(0, 0, 5)));



            //DrawEmenu
            Config.AddSubMenu(new Menu("Draw", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawE", "E Range").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawR", "R Range").SetValue(true));

           // Activator.addmenu(Config);

            Config.AddToMainMenu();
            // end menu

            if (Config.Item("skin").GetValue<bool>())
            {
                Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(Player.NetworkId, Config.Item("skin1").GetValue<Slider>().Value, Player.ChampionName)).Process();
                lastSkinId = Config.Item("skin1").GetValue<Slider>().Value;
            }

            Game.PrintChat("AlienHack [XinSharp - The Dragon Talon] Loaded!");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            LXOrbwalker.AfterAttack += AfterAttack;
        }

        static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe && QSpell.IsReady() && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo && Config.Item("UseQCombo").GetValue<bool>() && target.IsValidTarget(QSpell.Range)) 
            {
                LXOrbwalker.ResetAutoAttackTimer();
                QSpell.Cast(); 
            }
        }

        public static bool packets()
        {
            return Config.Item("packet").GetValue<bool>();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawE").GetValue<bool>() && ESpell.Level > 0) Utility.DrawCircle(Player.Position, ESpell.Range, System.Drawing.Color.Red);
            if (Config.Item("DrawR").GetValue<bool>() && RSpell.Level > 0) Utility.DrawCircle(Player.Position, RSpell.Range, System.Drawing.Color.Red);
        }

        static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("useRIn").GetValue<bool>()) return;
            if (RSpell.IsReady() && unit.IsValidTarget(RSpell.Range))
                RSpell.Cast();
        }

        static void Game_OnGameUpdate(EventArgs args)
        {


            //LXorbwalk

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear)
            {
                LaneClear();
            }
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
            {
                Combo();
            }

            ks();
            

            if (Config.Item("skin").GetValue<bool>() && Config.Item("skin1").GetValue<Slider>().Value != lastSkinId)
                {
                    Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(Player.NetworkId, Config.Item("skin1").GetValue<Slider>().Value, Player.ChampionName)).Process();
                    lastSkinId = Config.Item("skin1").GetValue<Slider>().Value;
                }

            if (Config.Item("usesmite").GetValue<KeyBind>().Active)
            {
                Smite();
            }

        }

		
        private static int getSmiteDmg()
        {
            int level = Player.Level;
            int index = Player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

		//Daibaths
        static void Smite()
        {
            string[] jungleMinions;
            if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.TwistedTreeline))
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[] { "AncientGolem", "LizardElder", "Worm", "Dragon" };
            }

            var minions = MinionManager.GetMinions(Player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = getSmiteDmg();
                foreach (Obj_AI_Base minion in minions)
                {

                    Boolean b;
                    if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.TwistedTreeline))
                    {
                        b = minion.Health <= smiteDmg &&
                            jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name));
                    }
                    else
                    {
                        b = minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name));
                    }

                    if (b)
                    {
                        Player.SummonerSpellbook.CastSpell(smiteSlot.Slot, minion);
                    }
                }
            }
        }

        static void ks()
        {


            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 600 && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);


            foreach (var target in nearChamps)
            {
                //ignite
                if (target != null && Config.Item("useIgnite").GetValue<bool>() && igniteSlot != SpellSlot.Unknown &&
                                Player.SummonerSpellbook.CanUseSpell(igniteSlot) == SpellState.Ready && Player.Distance(target.ServerPosition) <= 600)
                {
                    if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(igniteSlot, target);
                    }
                }

                if (Player.Distance(target.ServerPosition) <= ESpell.Range && (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health)
                {
                    if (ESpell.IsReady())
                    {
                        ESpell.Cast(target, packets());
                        return;
                    }
                }

            }


        }

        static void Combo()
        {
            var target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);


            if (Config.Item("UseECombo").GetValue<bool>() && ESpell.IsReady() && target.IsValidTarget(ESpell.Range) && !LXOrbwalker.InAutoAttackRange(target)) 
            {
                ESpell.Cast(target, packets());
            }

            if (Config.Item("UseWCombo").GetValue<bool>() && WSpell.IsReady() && LXOrbwalker.InAutoAttackRange(target))
            {
                WSpell.Cast();
            }
            if (Config.Item("UseRCombo").GetValue<bool>() && RSpell.IsReady() && (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health && target.IsValidTarget(RSpell.Range))
            {
                RSpell.Cast();
            }

            UseItemes(target);
        }

        static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, ESpell.Range,
                MinionTypes.All,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            
            if(minion.Count > 0){
                 var minions = minion[0];
             if (Config.Item("UseQLaneClear").GetValue<bool>() && QSpell.IsReady() && minions.IsValidTarget(QSpell.Range))
             {
                 QSpell.Cast();
             }

             if (Config.Item("UseWLaneClear").GetValue<bool>() && WSpell.IsReady() && LXOrbwalker.InAutoAttackRange(minions))
             {
                 WSpell.Cast();
             }
             if (Config.Item("UseELaneClear").GetValue<bool>() && ESpell.IsReady() && minions.IsValidTarget(ESpell.Range) )
             {
                 ESpell.Cast(minions, packets());
             }
        }

        }

       /* ADD later LX Still Bug :(
        * 
        * static void Flee()
        {

        }*/


      //Daibaths
        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = Config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth * (Config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBilgemyhp = Player.Health <=
                             (Player.MaxHealth * (Config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
            var iBlade = Config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth * (Config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBlademyhp = Player.Health <=
                             (Player.MaxHealth * (Config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
            var iOmen = Config.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              Config.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = Config.Item("Tiamat").GetValue<bool>();
            var iHydra = Config.Item("Hydra").GetValue<bool>();
            var ilotis = Config.Item("lotis").GetValue<bool>();
            //var ihp = Config.Item("Hppotion").GetValue<bool>();
            // var ihpuse = Player.Health <= (Player.MaxHealth * (Config.Item("Hppotionuse").GetValue<Slider>().Value) / 100);
            //var imp = Config.Item("Mppotion").GetValue<bool>();
            //var impuse = Player.Health <= (Player.MaxHealth * (Config.Item("Mppotionuse").GetValue<Slider>().Value) / 100);

            if (Player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && bilge.IsReady())
            {
                bilge.Cast(target);

            }
            if (Player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && blade.IsReady())
            {
                blade.Cast(target);

            }
            if (Utility.CountEnemysInRange(350) >= 1 && iTiamat && tiamat.IsReady())
            {
                tiamat.Cast();

            }
            if (Utility.CountEnemysInRange(350) >= 1 && iHydra && hydra.IsReady())
            {
                hydra.Cast();

            }
            if (iOmenenemys && iOmen && rand.IsReady())
            {
                rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (Config.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
                        hero.Distance(Player.ServerPosition) <= lotis.Range && lotis.IsReady())
                        lotis.Cast();
                }
            }
        }
   
    
    
    }

}