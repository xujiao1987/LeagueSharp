#region
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace Amumu
{
    internal class Program
    {
    	private static Menu Config;
    	private static Obj_AI_Hero Target;
        private static Obj_AI_Hero Player;
		private static Spell Q;
		private static Spell W;
		private static Spell E;
		private static Spell R;
		private static SpellSlot Smite;
		private static string[] jungleMinions;
		private static bool WPressed;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            jungleMinions = null;
            
           	if (Player.ChampionName != "Amumu") return;
           	        	
			Q = new Spell(SpellSlot.Q, 1100);
			W = new Spell(SpellSlot.W, 310);
			E = new Spell(SpellSlot.E, 320);
			R = new Spell(SpellSlot.R, 500);
			Smite = Player.GetSpellSlot("SummonerSmite"); 
			
			Q.SetSkillshot(0.5f, 80, 2000, true, SkillshotType.SkillshotLine);
                          							
			Config = new Menu("阿木木", "FA Amumu", true);
						
			var targetSelectorMenu = new Menu("目标选择", "Target Selector");
			SimpleTs.AddToMenu(targetSelectorMenu);
			Config.AddSubMenu(targetSelectorMenu);
			
            Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
            var orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
						
			Config.AddSubMenu(new Menu("连招", "Combo"));
			Config.SubMenu("Combo").AddItem(new MenuItem("Combo", "连招").SetValue(new KeyBind(32, KeyBindType.Press)));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "使用Q").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "使用W").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "使用E").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用R").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("REnemies", "R击中>").SetValue(new Slider(3,5,0)));
			
			Config.AddSubMenu(new Menu("补兵", "Farm"));
			Config.SubMenu("Farm").AddItem(new MenuItem("Clear", "补兵").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "使用W").SetValue(true));
			Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "使用E").SetValue(true));
						
			Config.AddSubMenu(new Menu("杂项", "Misc"));
			Config.SubMenu("Misc").AddItem(new MenuItem("Steal", "打野").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Misc").AddItem(new MenuItem("AutoW", "自动W").SetValue(true));
			Config.SubMenu("Misc").AddItem(new MenuItem("ManaW", "蓝量<%不W").SetValue(new Slider(15,100,0)));
			Config.SubMenu("Misc").AddItem(new MenuItem("QInterrupt", "Q打断").SetValue(true));
			Config.SubMenu("Misc").AddItem(new MenuItem("RInterrupt", "R打断").SetValue(true));
			Config.SubMenu("Misc").AddItem(new MenuItem("Packets", "封包").SetValue(true));
			           
            Config.AddSubMenu(new Menu("显示", "Drawings"));
			Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));	
			Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));	
			Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));	
			Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));	
			Config.AddToMainMenu();
			
			if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.TwistedTreeline))
				jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
			else jungleMinions = new string[] { "SRU_Blue", "SRU_Red", "SRU_Baron", "SRU_Dragon" };
				           
            Game.PrintChat("FA Amumu loaded!");

			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.OnWndProc += OnWndMsg;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private static bool GetBool(string s)
        {
        	return Config.Item(s).GetValue<bool>();
        }
        
        private static bool GetActive(string s)
        {
        	return Config.Item(s).GetValue<KeyBind>().Active;
        }
        
        private static LeagueSharp.Common.Circle GetCircle(string s)
        {
        	return Config.Item(s).GetValue<Circle>();
        }
        
        private static int GetSlider(string s)
        {
        	return Config.Item(s).GetValue<Slider>().Value;
        }
              
        private static void Game_OnGameUpdate(EventArgs args)
        {
			Target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
			AutoW();
			if (GetActive("Combo")) Combo();
			else if (GetActive("Clear")) Farm();
			if (GetActive("Steal")) JungSteal();
        }
                
        private static void Combo()
        {
        	if (GetBool("UseQ") && Q.IsReady() && Q.InRange(Target.Position)) Q.Cast(Target,GetBool("Packets"));
        	if (GetBool("UseW") && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.CountEnemysInRange((int)W.Range) >= 1
        	   		 && (Player.Mana/Player.MaxMana*100) > GetSlider("ManaW")) W.CastOnUnit(Player,GetBool("Packets"));
        	if (GetBool("UseE") && E.IsReady() && Player.CountEnemysInRange((int)E.Range) >= 1) E.CastOnUnit(Player,GetBool("Packets"));
        	if (GetBool("UseR") && Player.CountEnemysInRange((int)R.Range) >= GetSlider("REnemies")) R.CastOnUnit(Player,GetBool("Packets"));
        }
        
        private static int CountMinionsInRange(float range)
        {
        	int count = 0;
        	foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => Vector3.Distance(Player.Position, minion.Position) <= range  && minion.IsValid && !minion.IsDead))
        		count = count + 1 ;
            return count;
        }
        
        private static void Farm()
        {        	
        	if (GetBool("UseWFarm") && (Player.Mana/Player.MaxMana*100) > GetSlider("ManaW") && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && CountMinionsInRange(W.Range) >=1)
        	{
        		W.CastOnUnit(Player,GetBool("Packets"));
        	}
        	if (GetBool("UseEFarm") && E.IsReady() && CountMinionsInRange(E.Range) >=1)
        	{
        		E.CastOnUnit(Player,GetBool("Packets"));
        	}
        }
        
        private static void AutoW()
        {
        	if (GetBool("AutoW") && (Player.Mana/Player.MaxMana*100) > GetSlider("ManaW") && Player.CountEnemysInRange((int)W.Range) >= 1 && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && 
        	    (!Player.HasBuff("Recall") || !Player.IsWindingUp) && W.IsReady()) W.CastOnUnit(Player,GetBool("Packets"));
        	else if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2 && W.IsReady() && Player.CountEnemysInRange((int)W.Range) < 1 && CountMinionsInRange(W.Range) < 1 && !WPressed) 
        		W.CastOnUnit(Player,GetBool("Packets"));
        	else if (Player.CountEnemysInRange((int)W.Range) >= 1 || CountMinionsInRange(W.Range) >= 1 ) WPressed = false;
        }
        
        private static void MoveToMouse()
		{
			var Pos = Player.ServerPosition + 250 * (Game.CursorPos.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
			Player.IssueOrder(GameObjectOrder.MoveTo, Pos);
		}
        
        private static double SmiteDamage()
		{
			int level = Player.Level;
			int[] stages =
						{
							20*level + 370,
							30*level + 330,
							40*level + 240,
							50*level + 100
						};
			return stages.Max();
		}
        
        private static Obj_AI_Minion GetNearestMinionByNames(Vector3 pos, string[] names)
		{
			var minions = ObjectManager.Get<Obj_AI_Minion>().Where(minion =>minion.IsValid && names.Any(name => String.Equals(minion.SkinName, name, StringComparison.CurrentCultureIgnoreCase)));
			var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
			Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
			double? nearest = null;
			foreach (Obj_AI_Minion minion in objAiMinions)
			{			
				double distance = Vector3.Distance(pos, minion.Position);
				if (nearest == null || nearest > distance)
				{
					nearest = distance;
					sMinion = minion;
				}
			}
			return sMinion;
		}   
        
        private static bool CanUseSmite()
        {
        	return !Player.IsDead && !Player.IsStunned && Smite != SpellSlot.Unknown &&
					Player.SummonerSpellbook.CanUseSpell(Smite) == SpellState.Ready;		
        }
        
       	private static double CalculateDamage(Obj_AI_Minion target)
		{
			double damage = 0;
			if (Q.IsReady() && Q.InRange(target.Position))
				damage += Player.GetSpellDamage(target,SpellSlot.Q);
			if (E.IsReady() && E.InRange(target.Position))
				damage += Player.GetSpellDamage(target,SpellSlot.E);
			if (CanUseSmite() && Vector3.Distance(Player.Position, target.Position) <= 750)
				damage += SmiteDamage();
			return damage;
		}
               
        private static void JungSteal()
        {
        	if (jungleMinions == null) return;
			Obj_AI_Minion currentMinion = GetNearestMinionByNames(Player.Position,jungleMinions);
			if (currentMinion != null && currentMinion.IsValid && !currentMinion.IsDead && currentMinion.Health <= CalculateDamage(currentMinion))
			{
				if (Q.IsReady() && Q.InRange(currentMinion.Position)) Q.Cast(currentMinion,GetBool("Packets"));
				if (E.IsReady() && E.InRange(currentMinion.Position)) E.CastOnUnit(Player,GetBool("Packets"));
				if (CanUseSmite() && Vector3.Distance(Player.Position, currentMinion.Position) <= 750)
                   Player.SummonerSpellbook.CastSpell(Smite, currentMinion);					
			}       	
        }
        
        private static void OnWndMsg(WndEventArgs args)
        {
        	if (args.Msg == 0x100 && args.WParam == 0x57 && W.IsReady()) WPressed = true;
        }
        
        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
        	if (!GetBool("QInterrupt") && !GetBool("RInterrupt")) return;
        	if (GetBool("QInterrupt") && unit.IsValidTarget() && Q.InRange(unit.Position) && Q.IsReady()) Q.Cast(unit, GetBool("Packets"));
        	if (GetBool("RInterrupt") && unit.IsValidTarget() && R.InRange(unit.Position) && !Q.IsReady() && R.IsReady()) R.CastOnUnit(Player, GetBool("Packets"));
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
        	if (GetCircle("QRange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, Q.Range, GetCircle("QRange").Color);
        	if (GetCircle("WRange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, W.Range, GetCircle("WRange").Color);
            if (GetCircle("ERange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, E.Range, GetCircle("ERange").Color);
            if (GetCircle("RRange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, R.Range, GetCircle("RRange").Color);
        }   
    }
}
