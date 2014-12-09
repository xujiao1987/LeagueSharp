using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace SKO_Rengar_V2
{
	class Program
	{
		private static Obj_AI_Hero Player;
		private static Spell Q, W, E, R;
		private static Items.Item BWC, BRK, RO, YMG, STD, TMT, HYD;
		private static bool PacketCast;
		private static Menu SKOMenu;
		private static bool Recall; 
		private static SpellSlot IgniteSlot;
		private static readonly string[] JungleMinions =
		{
			"AncientGolem", "GreatWraith", "Wraith", "LizardElder", "Golem", "Worm", "Dragon", "GiantWolf"
		};

		public static void Main (string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}

		private static void Game_OnGameLoad(EventArgs args)
		{
			Player = ObjectManager.Player;
			if (Player.ChampionName != "Rengar")
				return;

			SKOMenu = new Menu ("狮子狗","SKORengar", true);

			var SKOTs = new Menu ("目标选择","TargetSelector");
			SimpleTs.AddToMenu(SKOTs);

			var OrbMenu = new Menu ("走砍", "Orbwalker");
			LXOrbwalker.AddToMenu (OrbMenu);


			var Combo = new Menu ("连招", "Combo");
			Combo.AddItem(new MenuItem("CPrio", "优先级").SetValue(new StringList(new[] {"Q", "W", "E"}, 2)));
			Combo.AddItem(new MenuItem ("UseQ", "使用 Q").SetValue(true));
			Combo.AddItem(new MenuItem ("UseW", "使用 W").SetValue(true));
			Combo.AddItem(new MenuItem ("UseE", "使用 E").SetValue(true));
			Combo.AddItem(new MenuItem ("UseR", "使用 R").SetValue(true));
			Combo.AddItem(new MenuItem ("UseItemsCombo", "使用点燃").SetValue(true));
			Combo.AddItem(new MenuItem ("UseAutoW", "自动 W").SetValue(true));
			Combo.AddItem(new MenuItem ("HpAutoW", "使用W最低HP在　").SetValue(new Slider(20,1,100)));
			Combo.AddItem (new MenuItem("TripleQ", "5怒双Q爆发连招(秒人)").SetValue(new KeyBind(OrbMenu.Item("Flee_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
			Combo.AddItem(new MenuItem("activeCombo", "连招").SetValue(new KeyBind(OrbMenu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var Harass = new Menu("骚扰", "Harass");
			Harass.AddItem(new MenuItem("HPrio", "优先级|").SetValue(new StringList(new[] {"W", "E"}, 1)));
			Harass.AddItem(new MenuItem("UseWH", "使用 W").SetValue(true));
			Harass.AddItem(new MenuItem("UseEH", "使用 E").SetValue(true));
			Harass.AddItem(new MenuItem ("UseItemsHarass", "5怒双Q爆发连招").SetValue(true));
			Harass.AddItem(new MenuItem("activeHarass","骚扰!").SetValue(new KeyBind(OrbMenu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var JLClear = new Menu("清线.清野", "JLClear");
			JLClear.AddItem(new MenuItem("FPrio", "优先级|").SetValue(new StringList(new[] {"Q", "W", "E"}, 0)));
			JLClear.AddItem(new MenuItem("UseQC", "使用 Q").SetValue(true));
			JLClear.AddItem(new MenuItem("UseWC", "使用 W").SetValue(true));
			JLClear.AddItem(new MenuItem("UseEC", "使用 E").SetValue(true));
			JLClear.AddItem(new MenuItem("UseItemsClear", "使用点燃").SetValue(true));
			JLClear.AddItem(new MenuItem("activeClear","清线|清野!").SetValue(new KeyBind(OrbMenu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var TROLLZINHONASRANKEDS = new Menu("抢人头", "TristanaKillZiggs");
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("Foguinho", "使用点燃").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseQKs", "使用 Q").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseWKs", "使用 W").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseEKs", "使用 E").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseFlashKs", "使用闪现").SetValue(false));

			var CHUPARUNSCUEPA = new Menu("范围", "Drawing");
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawQ", "范围 Q").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawW", "范围 W").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawE", "范围 E").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawR", "范围R").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleLag", "延迟自由圈|").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleQuality", "圈质量|").SetValue(new Slider(100, 100, 10)));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleThickness", "圈厚度|").SetValue(new Slider(1, 10, 1)));
           
			var Misc = new Menu("杂项", "Misc");
			Misc.AddItem(new MenuItem("UsePacket","使用封包").SetValue(true));
			
        

			SKOMenu.AddSubMenu(SKOTs);
			SKOMenu.AddSubMenu(OrbMenu);
			SKOMenu.AddSubMenu(Combo);
			SKOMenu.AddSubMenu(Harass);
			SKOMenu.AddSubMenu(JLClear);
			SKOMenu.AddSubMenu(TROLLZINHONASRANKEDS);
			SKOMenu.AddSubMenu(CHUPARUNSCUEPA);
			SKOMenu.AddSubMenu(Misc);
			SKOMenu.AddToMainMenu();

			W = new Spell(SpellSlot.W, 450f);
			E = new Spell(SpellSlot.E, 1000f);
			R = new Spell(SpellSlot.R, 2000f);

			E.SetSkillshot(.5f, 70f, 1500f, true, SkillshotType.SkillshotLine);

			HYD = new Items.Item(3074, 420f);
			TMT = new Items.Item(3077, 420f);
			BRK = new Items.Item(3153, 450f);
			BWC = new Items.Item(3144, 450f);
			RO = new Items.Item(3143, 500f);

			PacketCast = SKOMenu.Item("UsePacket").GetValue<bool>();

			IgniteSlot = Player.GetSpellSlot("SummonerDot");

			Game.OnGameUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Draw_OnDraw;
			Obj_AI_Hero.OnCreate += OnCreateObj;
			Obj_AI_Hero.OnDelete += OnDeleteObj;
		}

	
		private static void Game_OnGameUpdate(EventArgs args)
		{

			if(SKOMenu.Item("activeClear").GetValue<KeyBind>().Active)
			{
				Clear();
			}
			var tqtarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
			if(SKOMenu.Item("TripleQ").GetValue<KeyBind>().Active)
			{
				TripleQ(tqtarget);
			}

			var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

			Q = new Spell(SpellSlot.Q, Player.AttackRange+90);
			YMG = new Items.Item(3142, Player.AttackRange+50);
			STD = new Items.Item(3131, Player.AttackRange+50);

			AutoHeal();
			KillSteal(target);

			if(SKOMenu.Item("activeCombo").GetValue<KeyBind>().Active)
			{
				if(target.IsValidTarget())
				{
					if(Player.Mana <= 4)
					{	
						if(SKOMenu.Item("UseQ").GetValue<bool>() && Q.IsReady() && Player.Distance(target) <= Q.Range)
						{
							Q.Cast();
						}	
						if(SKOMenu.Item("UseW").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= W.Range){
							W.Cast();
						}
						if(SKOMenu.Item("UseE").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range){
							E.Cast(target, PacketCast);
						}
					}
					if(Player.Mana == 5)
					{
						if(SKOMenu.Item("UseQ").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 0 && Q.IsReady() && Player.Distance(target) <= Q.Range)
						{
							Q.Cast();
						}
						if(SKOMenu.Item("UseW").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 1 && W.IsReady() && Player.Distance(target) <= W.Range)
						{
							W.Cast();
						}
						if(SKOMenu.Item("UseE").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 2 && E.IsReady() && Player.Distance(target) <= E.Range)
						{
							E.Cast(target);
						}

						//E if !Q.IsReady()
						if(SKOMenu.Item("UseE").GetValue<bool>() && !Q.IsReady() && E.IsReady() && Player.Distance(target) > Q.Range)
						{
							E.Cast(target);
						}
					}
				if(SKOMenu.Item("UseItemsCombo").GetValue<bool>()){
					if(Player.Distance(target) < Player.AttackRange+50){
						TMT.Cast();
						HYD.Cast();
						STD.Cast();
					}
					BWC.Cast(target);
					BRK.Cast(target);
					RO.Cast(target);
					YMG.Cast();
					}

				}

			}
			if(SKOMenu.Item("activeHarass").GetValue<KeyBind>().Active)
			{
				Harass(target);
			}
				


		}
			

		private static void Draw_OnDraw(EventArgs args)
		{
			if (SKOMenu.Item("CircleLag").GetValue<bool>())
			{
				if (SKOMenu.Item("DrawQ").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, Q.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawW").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, W.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawE").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, E.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawR").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, R.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
			}
			else
			{
				if (SKOMenu.Item("DrawQ").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, Q.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawW").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, W.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawE").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, E.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawR").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, R.Range, Color.Green);
				}
			}
		}

		private static void KillSteal(Obj_AI_Hero target)
		{
			var IgniteDmg = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
			var QDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);
			var WDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);
			var EDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);

			if(target.IsValidTarget())
			{
				if(SKOMenu.Item("Foguinho").GetValue<bool>() && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
				{
					if(IgniteDmg > target.Health && Player.Distance(target) < 600)
					{
						Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
					}
				}
			}

			if(SKOMenu.Item("UseQKs").GetValue<bool>() && Q.IsReady() )
			{
				if(QDmg > target.Health && Player.Distance(target) <= Q.Range)
				{
					Q.Cast();
					LXOrbwalker.SetAttack(true);
				}
			}
			if(SKOMenu.Item("UseWKs").GetValue<bool>() && W.IsReady() )
			{
				if(WDmg > target.Health && Player.Distance(target) <= W.Range)
				{
					W.Cast();
				}
			}
			if(SKOMenu.Item("UseEKs").GetValue<bool>() && E.IsReady() )
			{
				if(EDmg > target.Health && Player.Distance(target) <= E.Range)
				{
					E.Cast(target, PacketCast);
				}
			}
		}


		private static void Clear()
		{
			var allminions = MinionManager.GetMinions(Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

			foreach(var minion in allminions)
			{
				if(minion.IsValidTarget()){
					if(Player.Mana <= 4)
					{
						if(Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && Player.Distance(minion) <= Q.Range)
						{
							Q.Cast();
						}
						if(W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && Player.Distance(minion) <= W.Range)
						{
							W.Cast();
						}
						if(E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && Player.Distance(minion) <= E.Range)
						{
							E.Cast(minion, PacketCast);
						}
					}
					if(Player.Mana == 5)
					{
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 0 && Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && Player.Distance(minion) <= Q.Range)
						{
							Q.Cast();
						}
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 1 && W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && Player.Distance(minion) <= W.Range)
						{
							W.Cast();
						}
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 2 && E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && Player.Distance(minion) <= E.Range)
						{
							E.Cast(minion, PacketCast);
						}
					}
					if(SKOMenu.Item("UseItemsClear").GetValue<bool>()){
						if(Player.Distance(minion) < Player.AttackRange+50){
							TMT.Cast();
							HYD.Cast();
						}
						YMG.Cast();
					}
				}
			}
		}

		private static void Harass(Obj_AI_Hero target)
		{
			if(target.IsValidTarget())
			{
				if(Player.Mana <= 4)
				{
					if(SKOMenu.Item("UseWH").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= W.Range){
						W.Cast();
					}
					if(SKOMenu.Item("UseEW").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range){
						E.Cast(target, PacketCast);
					}
				}
				if(Player.Mana == 5)
				{
					if(SKOMenu.Item("UseWH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 0 && W.IsReady())
					{
						W.Cast();
					}
					if(SKOMenu.Item("UseEH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 0 && E.IsReady()){
						E.Cast(target, PacketCast);
					}
				}
			if(SKOMenu.Item("UseItemsHarass").GetValue<bool>()){
				if(Player.Distance(target) < Player.AttackRange+50){
					TMT.Cast();
					HYD.Cast();
					STD.Cast();
				}
				BWC.Cast(target);
				BRK.Cast(target);
				RO.Cast(target);
				YMG.Cast();

			}
			}
		}
			

		private static void TripleQ(Obj_AI_Hero target)
		{
			if(target.IsValidTarget()){
				if(Player.Mana == 5 && R.IsReady() && Player.Distance(target) <= R.Range && Q.IsReady())
			{
				R.Cast();
			}
			if(Player.Mana < 5)
			{
				Drawing.DrawText(Player.Position.X, Player.Position.Y, Color.Red, "R is not ready, or you do not have 5 ferocity");
			}


			if(Player.Mana == 5 && Player.HasBuff("RengarR") && Q.IsReady() && Player.Distance(target) <= Q.Range)
			{
				Q.Cast();
			}
			if(Player.Mana == 5 && !Player.HasBuff("RengarR") && Q.IsReady() && Player.Distance(target) <= Q.Range)
			{
				Q.Cast();
			}
			if(Player.Mana <= 4)
			{
				if(Q.IsReady() && Player.Distance(target) <= Q.Range)
				{
					Q.Cast();
				}
				if(W.IsReady() && Player.Distance(target) <= W.Range)
				{
					W.Cast();
				}
				if(E.IsReady() && Player.Distance(target) <= E.Range)
				{
						E.Cast(target, PacketCast);
				}
			}
				if(Player.Distance(target) < Player.AttackRange+50){
					TMT.Cast();
					HYD.Cast();
					STD.Cast();
				}
				BWC.Cast(target);
				BRK.Cast(target);
				RO.Cast(target);
				YMG.Cast();

		}
		}
			

		private static void AutoHeal()
		{
			if(Player.Mana <= 4)return;

			if(SKOMenu.Item("UseAutoW").GetValue<bool>() && Player.Mana == 5 && !Recall)
			{
				if(W.IsReady() && Player.Health < (Player.MaxHealth * (SKOMenu.Item("HpAutoW").GetValue<Slider>().Value) / 100))
				{
					W.Cast();
				}
			}
		}

		private static void OnCreateObj(GameObject sender, EventArgs args)
		{
			//Recall
			if (!(sender is Obj_GeneralParticleEmmiter)) return;
			var obj = (Obj_GeneralParticleEmmiter)sender;
			if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
			{
				Recall = true;
			}
		}
		private static void OnDeleteObj(GameObject sender, EventArgs args)
		{
			//Recall
			if (!(sender is Obj_GeneralParticleEmmiter)) return;
			var obj = (Obj_GeneralParticleEmmiter)sender;
			if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
			{
				Recall = false;
			}
		}
			
	}
}
