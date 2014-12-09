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


namespace TryndSharp
{
    internal class TryndSharp
    {

        public const string CharName = "Tryndamere";

        public static Menu Config;

        public static Obj_AI_Hero target;

        public TryndSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Tryndamere - Sharp by DeTuKs");

            try
            {

                Config = new Menu("蛮族之王", "Tryndamere", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
                Trynd.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("目标选择", "Target Selector");
                SimpleTs.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("连招", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "使用点燃")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "使用 W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "使用 E")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("QonHp", "剩余血量使用Q")).SetValue(new Slider(25, 100, 0));
               // Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R on %")).SetValue(new Slider(25, 100, 0));

                //LastHit
                Config.AddSubMenu(new Menu("补兵", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("清兵", "lClear"));
               
               
                //Extra
                Config.AddSubMenu(new Menu("额外", "extra"));

                //Debug
                Config.AddSubMenu(new Menu("调试", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "调试英雄")).SetValue(new KeyBind('T', KeyBindType.Press, false));

                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {

            if (Trynd.orbwalker.ActiveMode.ToString() == "Combo")
            {
              //  Console.WriteLine("emm");
                if(Trynd.E.IsReady())
                    target = SimpleTs.GetTarget(950, SimpleTs.DamageType.Physical);
                else if (Trynd.W.IsReady())
                    target = SimpleTs.GetTarget(450, SimpleTs.DamageType.Physical);
                else
                    target = SimpleTs.GetTarget(250, SimpleTs.DamageType.Physical);

                Trynd.doCombo(target);
            }

            if (Trynd.orbwalker.ActiveMode.ToString() == "Mixed")
            {
               
            }

            if (Trynd.orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                
            }


            if (Config.Item("harassOn").GetValue<bool>() && Trynd.orbwalker.ActiveMode.ToString() == "None")
            {
              
            }
        }

        private static void onDraw(EventArgs args)
        {
            Drawing.DrawCircle(Trynd.Player.Position, Trynd.E.Range, Color.Blue);
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
