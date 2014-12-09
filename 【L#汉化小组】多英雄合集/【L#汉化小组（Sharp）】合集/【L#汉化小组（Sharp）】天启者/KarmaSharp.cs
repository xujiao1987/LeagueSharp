using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

/*
 * ToDo:
 * 
 * */


namespace KarmaSharp
{
    internal class KarmaSharp
    {

        public const string CharName = "Karma";

        public static Menu Config;

        public static Obj_AI_Hero target;

        public KarmaSharp()
        {
            if (ObjectManager.Player.BaseSkinName != CharName)
                return;
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Karma - Sharp by DeTuKs");

            try
            {

                Config = new Menu("︱天启者─卡尔玛︱", "Karma", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
                Karma.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("目标选择", "Target Selector");
                SimpleTs.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("连招", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "使用 Q")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "使用 W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "使用 E 给自己ㄧ")).SetValue(false);
                Config.SubMenu("combo").AddItem(new MenuItem("useR", "使用R与Q(骚扰也是)")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("补兵", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("清兵", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("骚扰", "harass"));
                Config.SubMenu("harass").AddItem(new MenuItem("harP", "骚扰敌人")).SetValue(new KeyBind('T', KeyBindType.Press, false));
                Config.SubMenu("harass").AddItem(new MenuItem("harT", "按键切换")).SetValue(new KeyBind('H', KeyBindType.Toggle, false));
                Config.SubMenu("harass").AddItem(new MenuItem("useQHar", "使用Q与R")).SetValue(true);
                //Extra
                Config.AddSubMenu(new Menu("额外", "extra"));
                Config.SubMenu("extra").AddItem(new MenuItem("useMinions", "对小兵使用Q")).SetValue(true);
				//Donate
        
                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                Karma.setSkillShots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Karma.orbwalker.ActiveMode.ToString() == "Combo")
            {
                target = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);
                    Karma.doCombo(target);
            }

            if (Karma.orbwalker.ActiveMode.ToString() == "Mixed")
            {
               
            }

            if (Karma.orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                
            }


            if (Config.Item("harP").GetValue<KeyBind>().Active || Config.Item("harT").GetValue<KeyBind>().Active)
            {
                target = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);
                    Karma.doHarass(target);
            }
        }

        private static void onDraw(EventArgs args)
        {
            Drawing.DrawCircle(Karma.Player.Position, 950, Color.Blue);
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
          

        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
          
        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {


           
        }




    }
}
