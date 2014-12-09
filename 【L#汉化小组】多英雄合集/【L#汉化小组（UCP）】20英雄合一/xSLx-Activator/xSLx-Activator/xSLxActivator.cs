﻿using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xSLx_Activator
{
	// ReSharper disable once InconsistentNaming
    public class xSLxActivator
    {
		public static Menu Menu;
		public static Obj_AI_Hero MyHero = ObjectManager.Player;

		public static void AddToMenu(Menu menu)
	    {
			var menuPotions = new Menu("药水", "xSLxActivator_Potions");
			// ReSharper disable once ObjectCreationAsStatement
			new PotionManager(menuPotions);
			menu.AddSubMenu(menuPotions);

			var menuItems = new Menu("物品", "xSLxActivator_Item");
			// ReSharper disable once ObjectCreationAsStatement
			new ItemManager(menuItems);
			menu.AddSubMenu(menuItems);

			var menuSummoners = new Menu("召唤师技能", "xSLxActivator_Summoner");
			// ReSharper disable once ObjectCreationAsStatement
			new SummonerManager(menuSummoners);
			menu.AddSubMenu(menuSummoners);
	    }
    }
}
