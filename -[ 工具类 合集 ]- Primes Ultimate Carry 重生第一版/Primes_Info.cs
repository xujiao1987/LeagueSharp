using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class PrimesInfo
	{
		internal static void AddtoMenu(Menu menu)
		{
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("info_sep0", "====== Information ======"));
			tempMenu.AddItem(new MenuItem("info_Patchnotes", "= Patchnotes").SetValue((new KeyBind("U".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_PUC", "= PUC Info").SetValue((new KeyBind("I".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_Champ", "= Champ Info").SetValue((new KeyBind("L".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_sep1", "===================="));
			PUC.Menu.AddSubMenu(tempMenu);

		}

		public static void Draw()
		{
			if(PUC.Menu.Item("info_Patchnotes").GetValue<KeyBind>().Active)
				InfoWindow.Patchnodes();
			if(PUC.Menu.Item("info_PUC").GetValue<KeyBind>().Active)
				InfoWindow.PUCInfo();
			if(PUC.Menu.Item("info_Champ").GetValue<KeyBind>().Active)
				InfoWindow.Champinfo();
		}
	}
}
