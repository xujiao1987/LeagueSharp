using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SigmaAutoPink
{
    class Program
    {
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
            LeagueSharp.GameObject.OnCreate += GameObject_OnCreate;
        }
        public static List<GameObject> wardList = new List<GameObject>();
        public static List<GameObject> akaliShroud = new List<GameObject>(); 

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "akali_smoke_bomb_tar_team_red.troy")
            {
                akaliShroud.Add(sender);
            }
            if (sender.Name == "VisionWard")
            {
                wardList.Add(sender);
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("active").GetValue<KeyBind>().Active)
            {
                foreach (var player in getEnemies())
                {
                    if (player.HasBuffOfType(BuffType.Invisibility) && player.BaseSkinName != "Evelynn")
                    {
                        if (Items.HasItem(3364) && Items.CanUseItem(3364))
                        {
                            if (Player.Distance(player) < 900)
                            {
                                if (Player.Distance(player) < 600)
                                {
                                    Items.UseItem(3364, Player.Position);
                                    
                                }
                                else
                                {
                                    Items.UseItem(3364, Vector3.Lerp(Player.Position, player.Position, 600 / Player.Distance(player)));
                                }
                            }
                        }
                        else if (Items.HasItem(2043))
                        {
                            var castward = true;
                            foreach (var ward in wardList)
                            {
                                if (Player.Distance(ward.Position) < 600)
                                {
                                    castward = false;
                                }
                            }
                            if (Player.Distance(player) < 600 && castward)
                            {
                                Items.UseItem(2043, player.Position);
                            }
                        }
                    }
                }
                    if (akaliShroud.Count > 0)
                    {
                        foreach (var shroud in akaliShroud)
                        {
                            if (Items.HasItem(3364) && Items.CanUseItem(3364))
                            {
                                if (Player.Distance(shroud.Position) < 900)
                                {
                                    if (Player.Distance(shroud.Position) < 600)
                                    {
                                        Items.UseItem(3364, Player.Position);
                                        akaliShroud.Remove(shroud);

                                    }
                                    else
                                    {
                                        Items.UseItem(3364, Vector3.Lerp(Player.Position, shroud.Position, 600 / Player.Distance(shroud.Position)));
                                        akaliShroud.Remove(shroud);
                                    }
                                }
                            }
                            else if (Items.HasItem(2043))
                            {
                                var castward = true;
                                foreach (var ward in wardList)
                                {
                                    if (Player.Distance(ward.Position) < 600)
                                    {
                                        castward = false;
                                    }
                                }
                                if (Player.Distance(shroud.Position) < 600 && castward)
                                {
                                    Items.UseItem(2043, shroud.Position);
                                    akaliShroud.Remove(shroud);
                                }
                            }
                        }
                    }
                
            }
        }

        public static IEnumerable<Obj_AI_Hero> getEnemies()
        {
            var enemies = from enemy in ObjectManager.Get<Obj_AI_Hero>()
                          where !enemy.IsAlly && ObjectManager.Player.Distance(enemy) < 2000
                          select enemy;
            return enemies;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Config = new Menu("SigmaPinkWard", "真眼", true);
            Config.AddSubMenu(new Menu("打开", "Active"));
            Config.SubMenu("Active").AddItem(new MenuItem("active", "打开").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.AddToMainMenu();
        }
    }
}
