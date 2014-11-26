using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class PUC
	{
		public static Obj_AI_Hero Player = ObjectManager.Player;
		public static Champion Champion;
		public static IEnumerable<Obj_AI_Hero> AllHeros = ObjectManager.Get<Obj_AI_Hero>();
		public static IEnumerable<Obj_AI_Hero> AllHerosFriend = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);
		public static IEnumerable<Obj_AI_Hero> AllHerosEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
		
		//public Champion Champion;
		//public Orbwalker Orbwalker;

		public static Menu Menu;

		public PUC()
		{

			Game.PrintChat("======  Primes Ultimate Carry Loaded! ======");
			Game.PrintChat("Beta version v 0.15");
			Game.PrintChat("This is a Beta version, not all is active,");
			Game.PrintChat("=================================");

			Player = ObjectManager.Player;
			Menu = new Menu("Primes Ultimate Carry", Player.ChampionName + "UltimateCarry", true);

			var infoMenu = new Menu("Primes Information", "Primes_Info");
			PrimesInfo.AddtoMenu(infoMenu);

			var sidebarMenu = new Menu("Primes SideBar", "Primes_SideBar");
			SideBar.AddtoMenu(sidebarMenu);

			var trackerMenu = new Menu("Primes Tracker", "Primes_Tracker");
			Tracker.AddtoMenu(trackerMenu);

			var tsMenu = new Menu("Primes TargetSelector", "Primes_TS");
			TargetSelector.AddtoMenu(tsMenu);

			var orbwalkMenu = new Menu("Primes Orbwalker", "Primes_Orbwalker");
			Orbwalker.AddtoMenu(orbwalkMenu);

			var activatorMenu = new Menu("Primes Activator", "Primes_Activator");
			Activator.AddtoMenu(activatorMenu);

			var autolevelMenu = new Menu("Primes AutoLevel", "Primes_AutoLevel");
			AutoLevel.AddtoMenu(autolevelMenu);
			var loadbaseult = false;
			switch(Player.ChampionName)
			{
				case "Ashe":
					loadbaseult = true;
					break;
				case "Draven":
					loadbaseult = true;
					break;
				case "Ezreal":
					loadbaseult = true;
					break;
				case "jinx":
					loadbaseult = true;
					break;
			}
			if(loadbaseult)
			{
				var baseUltMenu = new Menu("Primes BaseUlt", "Primes_BaseUlt");
				BaseUlt.AddtoMenu(baseUltMenu);
			}

		//if(Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift ||
			//	Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
			//{
			//	var tarzanMenu = new Menu("Primes Tarzan", "Primes_Tarzan");
			//	Jungle.AddtoMenu(tarzanMenu);
			//}

			LoadChampionPlugin();

			Menu.AddToMainMenu();

			Drawing.OnDraw += Drawing_OnDraw;
		}

		private void LoadChampionPlugin()
		{

			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var handle = System.Activator.CreateInstance(null, "Primes_Ultimate_Carry.Champion_" + ObjectManager.Player.ChampionName);
				Champion = (Champion)handle.Unwrap();
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch(Exception)
			{			
			}

		}

		private void Drawing_OnDraw(EventArgs args)
		{
			PrimesInfo.Draw();
			TargetSelector.Draw();
			SideBar.Draw();
			Orbwalker.Draw();
			//if(Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift ||
			//	Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
			//{
			//	Jungle.Draw();
			//}
			Activator.Draw();
		}
	}
}
