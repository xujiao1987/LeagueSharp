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
 * Hydra
 * 
 * overkill
 * 
 * 
 * */


namespace KhazixSharp
{
    internal class KhazixSharp
    {

        public const string CharName = "Khazix";

        public static Menu Config;

        public static HpBarIndicator hpi = new HpBarIndicator();

        public KhazixSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Khazix - Sharp by DeTuKs");

            try
            {

                Config = new Menu("Khazix", "Khazix", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("璧扮爫", "Orbwalker"));
                Khazix.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
                SimpleTs.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("杩炴嫑", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "浣跨敤鐗╁搧")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("琛ュ叺", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("娓呯嚎", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("楠氭壈", "harass"));
               
                //Extra
                Config.AddSubMenu(new Menu("鍏朵粬", "extra"));
                

                //Debug
                Config.AddSubMenu(new Menu("璋冭瘯", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "淇鐩爣")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                GameObject.OnPropertyChange += OnPropertyChange;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
                Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;

                Game.OnGameSendPacket += OnGameSendPacket;
                Game.OnGameProcessPacket += OnGameProcessPacket;

                Khazix.setSkillshots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with KhazixSharp");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            try
            {

            if (Khazix.orbwalker.ActiveMode.ToString() == "Combo")
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(Khazix.getBestRange(), SimpleTs.DamageType.Physical);

                Khazix.checkUpdatedSpells();


                Khazix.doCombo(target);
                //Console.WriteLine(target.NetworkId);
            }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private static void onDraw(EventArgs args)
        {
            foreach (
                            var enemy in
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                hpi.unit = enemy;
                hpi.drawDmg(Khazix.fullComboDmgOn(enemy), Color.Yellow);
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            

        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {

        }



        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base sender, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
           
        }

        public static void OnPropertyChange(LeagueSharp.GameObject obj, LeagueSharp.GameObjectPropertyChangeEventArgs prop)
        {

        }

        public static void OnPlayAnimation(LeagueSharp.GameObject value0, GameObjectPlayAnimationEventArgs value1)
        {
            
        }


        public static void OnGameProcessPacket(GamePacketEventArgs args)
        {

        }

        public static void OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args != null && (args.PacketData[0] == 175))
            {
                //Console.WriteLine("aa " + args.PacketData[0]);
               // args.Process = false;
            }
        }




    }
}
