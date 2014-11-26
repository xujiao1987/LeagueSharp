#region
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D9;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using System.Globalization;
using System.Threading;
#endregion

namespace LightningRyze
{
    internal class Program
    {
        private static Menu Config;
        private static string LastCast;
        private static float LastFlashTime;
        private static Obj_AI_Hero target;
        private static Obj_AI_Hero myHero;
        private static bool UseShield;
       	private static Spell Q;
		private static Spell W;
		private static Spell E;
		private static Spell R;
		private static SpellSlot IgniteSlot;
        public static HpBarIndicator hpi = new HpBarIndicator();
        private static readonly List<Hero> _heroes = new List<Hero>();
		
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
          
        private static void Game_OnGameLoad(EventArgs args)
        {
            myHero = ObjectManager.Player;
            Game.PrintChat("============================");
            Game.PrintChat("Loading TwilightRyze! ....");
            if (myHero.ChampionName != "Ryze")
            {
                Game.PrintChat("Twilight Ryze loading failed! (Incorrect champion!)");
                return;
            }
           	
       		Q = new Spell(SpellSlot.Q, 625);
			W = new Spell(SpellSlot.W, 600);
			E = new Spell(SpellSlot.E, 600);
			R = new Spell(SpellSlot.R);
			IgniteSlot = myHero.GetSpellSlot("SummonerDot");  
			
			Config = new Menu("Lightning Ryze", "Twilight Ryze", true);
			var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
			SimpleTs.AddToMenu(targetSelectorMenu);
			Config.AddSubMenu(targetSelectorMenu);
			
            var orbwalkerMenu = new Menu("LX-Orbwalker", "Orbalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);
			
			Config.AddSubMenu(new Menu("Combo", "Combo"));
			Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
			Config.SubMenu("Combo").AddItem(new MenuItem("TypeCombo", "").SetValue(new StringList(new[] {"Mixed mode","Burst combo","Long combo"},0)));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
            
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Harass").AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
			Config.SubMenu("Harass").AddItem(new MenuItem("HW", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HE", "Use E").SetValue(true));
            
            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Farm").AddItem(new MenuItem("FQ", "Use Q").SetValue(true));
			Config.SubMenu("Farm").AddItem(new MenuItem("FW", "Use W").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("FE", "Use E").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("FR", "Use R").SetValue(true));

            Config.AddSubMenu(new Menu("Exploit", "Exploit"));
            Config.SubMenu("Exploit").AddItem(new MenuItem("UsePacket", "Use Packet Cast").SetValue(true));
            Config.SubMenu("Exploit").AddItem(new MenuItem("tearStack", "Q+W duoble tear effect (BETA)").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungActive", "Farm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("JungleFarm").AddItem(new MenuItem("JQ", "Use Q").SetValue(true));
			Config.SubMenu("JungleFarm").AddItem(new MenuItem("JW", "Use W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JE", "Use E").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JR", "Use R").SetValue(true));
            
            Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Use Kill Steal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("AutoIgnite", "Use Ignite").SetValue(true));
            
            Config.AddSubMenu(new Menu("Extra", "Extra"));
            Config.SubMenu("Extra").AddItem(new MenuItem("UseSera", "Use Seraphs Embrace").SetValue(true));
            Config.SubMenu("Extra").AddItem(new MenuItem("HP", "When % HP").SetValue(new Slider(20, 100, 0)));
            Config.SubMenu("Extra").AddItem(new MenuItem("UseWGap", "Use W GapCloser").SetValue(true));
            Config.SubMenu("Extra").AddItem(new MenuItem("AutoPoke", "AutoHarass (Toggle)").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Extra").AddItem(new MenuItem("ManaFreeze", "MinMana % Harass").SetValue(new Slider(40, 1, 100)));
            Config.SubMenu("Extra").AddItem(new MenuItem("WInterruptSpell", "Interrupt spells W").SetValue(true));
            
            Config.AddSubMenu(new Menu("MapHack", "MapHack"));
            Config.SubMenu("MapHack").AddItem(new MenuItem("TextColorMH", "Text Color").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("MapHack").AddItem(new MenuItem("OutlineColorMH", "Outline Color").SetValue(new Circle(true, Color.FromArgb(255, 0, 0, 0))));
            Config.SubMenu("MapHack").AddItem(new MenuItem("MapHack", "Enabled").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
			Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("WERange", "W+E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawDamage", "Calculate damage to target").SetValue(true));
			Config.AddToMainMenu();

            Game.PrintChat("TwilightRyze loaded!");
            Game.PrintChat("============================");


			Game.OnGameUpdate += Game_OnGameUpdate;
			LXOrbwalker.BeforeAttack += LXOrbwalker_BeforeAttack;
			Drawing.OnDraw += Drawing_OnDraw;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnEndScene += OnEndScene;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.Item("drawDamage").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    hpi.unit = enemy;
                    hpi.drawDmg(GetComboDamage(enemy), Color.Yellow);
                }
            }
        }
    
        private static void Game_OnGameUpdate(EventArgs args)
        {         
        	target = SimpleTs.GetTarget(Q.Range+25, SimpleTs.DamageType.Magical);
			if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
			{
				if (Config.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 0) ComboMixed();
				else if (Config.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 1) ComboBurst();
				else if (Config.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 2) ComboLong();
			}
			else if (Config.Item("HarassActive").GetValue<KeyBind>().Active) Harass();
			else if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active ||
			    Config.Item("FreezeActive").GetValue<KeyBind>().Active) Farm();
			else if (Config.Item("JungActive").GetValue<KeyBind>().Active) JungleFarm();
            if (Config.Item("UseSera").GetValue<bool>()) UseItems();
            if (Config.Item("tearStack").GetValue<KeyBind>().Active) TearExploit();
            if (Config.Item("AutoPoke").GetValue<KeyBind>().Active) AutoPoke();

            try
            {
                foreach (
                    Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
                {
                    if (_heroes.All(t => t.Name != hero.BaseSkinName))
                    {
                        _heroes.Add(new Hero
                        {
                            Name = hero.BaseSkinName,
                            Visible = true,
                            Dead = hero.IsDead,
                            LastPosition = hero.Position
                        });
                    }
                    Hero h = _heroes.FirstOrDefault(heroes => heroes.Name == hero.BaseSkinName);
                    if (h != null)
                    {
                        h.Visible = hero.IsVisible;
                        h.Dead = hero.IsDead;
                        h.LastPosition = hero.IsVisible ? hero.Position : h.LastPosition;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
        	var drawQ = Config.Item("QRange").GetValue<Circle>();
            if (drawQ.Active && !myHero.IsDead)
            {
                Utility.DrawCircle(myHero.Position, Q.Range, drawQ.Color);
            }

            var drawWE = Config.Item("WERange").GetValue<Circle>();
            if (drawWE.Active && !myHero.IsDead)
            {
                Utility.DrawCircle(myHero.Position, W.Range, drawWE.Color);
            }
            try
            {
                if (Config.Item("drawDamage").GetValue<bool>())
                {
                    if (target != null && !target.IsDead && !myHero.IsDead)
                    {
                        var ts = target;
                        var wts = Drawing.WorldToScreen(target.Position);
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Total damage: " + GetComboDamage(target) + "!");
                        if (GetComboDamage(target) >= ts.Health)
                        {
                            Drawing.DrawText(wts[0] - 40, wts[1] + 70, Color.OrangeRed, "Status: Killable");
                        }
                        else if (GetComboDamage(target) < ts.Health)
                        {
                            Drawing.DrawText(wts[0] - 40, wts[1] + 70, Color.OrangeRed, "Status: Needs harass!");
                        }

                    }
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                    {
                        hpi.unit = enemy;
                        if (GetComboDamage(enemy) >= enemy.Health)
                        {
                            hpi.drawDmg(GetComboDamage(enemy), Color.Red);
                        }
                        else
                        {
                            hpi.drawDmg(GetComboDamage(enemy), Color.Yellow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.PrintChat("Failed to draw HP bar damage! => " + ex);
            }

            if (Config.Item("MapHack").GetValue<bool>())
            {
                try
                {
                    foreach (Hero hero in _heroes)
                    {
                        if (!hero.Dead && !hero.Visible)
                        {
                            Vector2 pos = Drawing.WorldToMinimap(hero.LastPosition);

                            var OutlineColor = Config.Item("OutlineColorMH").GetValue<Circle>();
                            var TextColor = Config.Item("TextColorMH").GetValue<Circle>();

                            Drawing.DrawText(pos.X - Convert.ToInt32(hero.Name.Substring(0, 3).Length * 5 - 1), pos.Y - 6, OutlineColor.Color, hero.Name.Substring(0, 3));
                            Drawing.DrawText(pos.X - Convert.ToInt32(hero.Name.Substring(0, 3).Length * 5 + 1), pos.Y - 8, OutlineColor.Color, hero.Name.Substring(0, 3));
                            Drawing.DrawText(pos.X - Convert.ToInt32(hero.Name.Substring(0, 3).Length * 5 + 1), pos.Y - 6, OutlineColor.Color, hero.Name.Substring(0, 3));
                            Drawing.DrawText(pos.X - Convert.ToInt32(hero.Name.Substring(0, 3).Length * 5 - 1), pos.Y - 8, OutlineColor.Color, hero.Name.Substring(0, 3));

                            Drawing.DrawText(pos.X - Convert.ToInt32(hero.Name.Substring(0, 3).Length * 5), pos.Y - 7, TextColor.Color, hero.Name.Substring(0, 3));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public static float GetManaPerc(Obj_AI_Base unit)
        {
            return (unit.Mana / unit.MaxMana) * 100;
        }
        private static void AutoPoke()
        {
            var UsePacket = Config.Item("UsePacket").GetValue<bool>(); 
            var eTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var ManaFreeze = Config.Item("ManaFreeze").GetValue<Slider>().Value;
            if (eTarget == null) 
                return;

            if (eTarget.IsValidTarget(Q.Range) && Q.IsReady() && GetManaPerc(myHero) > ManaFreeze) 
                Q.CastOnUnit(eTarget, UsePacket);
        }
        // Q+W
        private static void TearExploit()
        {
            var UsePacket = Config.Item("UsePacket").GetValue<bool>();
            var allMinions = MinionManager.GetMinions(myHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            var eTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (Q.IsReady() && W.IsReady())
            {
                double delay;
                int myPing = Game.Ping;

                foreach (var minion in allMinions)
                {
                    if (myHero.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                    {
                        // Q Range = 625
                        // Q speed = Distance/~60
                        delay = (minion.Distance(myHero)*60)/625;
                        //Game.PrintChat("Distance: " + minion.Distance(myHero) + " Delay:" + Convert.ToInt32(delay));
                        Q.CastOnUnit(minion, UsePacket);
                        Utility.DelayAction.Add(Convert.ToInt32(delay), () => W.CastOnUnit(minion, UsePacket));
                    }
                    break;
                }
            }
        }
       	private static void LXOrbwalker_BeforeAttack(LXOrbwalker.BeforeAttackEventArgs args)
		{
			if (Config.Item("ComboActive").GetValue<KeyBind>().Active || Config.Item("HarassActive").GetValue<KeyBind>().Active)
				args.Process = !(Q.IsReady() || W.IsReady() || E.IsReady() || myHero.Distance(args.Target) >= 600);
		}
        
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
        	var UseW = Config.Item("UseWGap").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
			if (myHero.HasBuff("Recall") || myHero.IsWindingUp) return;  
        	if (UseW && W.IsReady()) W.CastOnUnit(gapcloser.Sender,UsePacket);
        }
        
        
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        	if (sender.IsMe)
        	{
        		if (args.SData.Name.ToLower() == "overload")
				{
					LastCast = "Q";
				}
				else if (args.SData.Name.ToLower() == "runeprison")
				{
					LastCast = "W";
				}
				else if (args.SData.Name.ToLower() == "spellflux")
				{
					LastCast = "E";
				}
				else if (args.SData.Name.ToLower() == "desperatepower")
				{
					LastCast = "R";
				}
				else if (args.SData.Name.ToLower() == "summonerflash")
        		{
        			LastFlashTime = Environment.TickCount;
        		}
        	}	
        	if (sender.IsEnemy && (sender.Type == GameObjectType.obj_AI_Hero || sender.Type == GameObjectType.obj_AI_Turret))
        	{
        		if (SpellData.SpellName.Any(Each => Each.Contains(args.SData.Name)) || (args.Target == myHero && myHero.Distance(sender) <= 700))
        			UseShield = true;
        	}
        }
        
        private static bool IsFacing(Obj_AI_Base source, Obj_AI_Base target)
		{
			if (!source.IsValid || !target.IsValid) return false;			
			if (source.Path.Count() > 0 && source.Path[0].Distance(target.ServerPosition) < target.Distance(source))
				return true;
			else return false;				
		}
               
       	private static bool IgniteKillable(Obj_AI_Base target)
       	{       		
       		return Damage.GetSummonerSpellDamage(myHero, target,Damage.SummonerSpell.Ignite) > target.Health;
       	}       
       	
       	private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += myHero.GetSpellDamage(enemy, SpellSlot.Q)*2;
            if (W.IsReady())
                damage += myHero.GetSpellDamage(enemy, SpellSlot.W);
            if (E.IsReady())
                damage += myHero.GetSpellDamage(enemy, SpellSlot.E);
			if (IgniteSlot != SpellSlot.Unknown && myHero.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready) 
				damage += myHero.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            return (float)damage;
        }
              	
       	private static float GetDistanceSqr(Obj_AI_Hero source, Obj_AI_Base target)
       	{
       		return Vector2.DistanceSquared(source.Position.To2D(),target.ServerPosition.To2D());
       	}
       	
       	private static void UseItems()
       	{
       		var myHP = myHero.Health/myHero.MaxHealth*100;
       		var ConfigHP = Config.Item("HP").GetValue<Slider>().Value;
       		if (myHP <= ConfigHP && Items.HasItem(3040) && Items.CanUseItem(3040) && UseShield) 
       		{
       			Items.UseItem(3040);
       			UseShield = false;
       		}
       	}
        
       	private static void ComboMixed()
        {
        	var UseR = Config.Item("UseR").GetValue<bool>();
        	var UseIgnite = Config.Item("UseIgnite").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	if (target == null) return;
        	
        	if (UseIgnite && (IgniteKillable(target) || GetComboDamage(target) > target.Health))
        		if (IgniteSlot != SpellSlot.Unknown && myHero.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && myHero.Distance(target) <= 600)
					myHero.SummonerSpellbook.CastSpell(IgniteSlot, target);    
        	
        	if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target,UsePacket);
        	else
        	{
        		if (Q.IsKillable(target) && Q.IsReady()) Q.CastOnUnit(target,UsePacket);
        		else if (E.IsKillable(target) && E.IsReady()) E.CastOnUnit(target,UsePacket); 
        		else if (W.IsKillable(target) && W.IsReady()) W.CastOnUnit(target,UsePacket);  
        		else if (myHero.Distance(target) >= 575 && !IsFacing(target,myHero) && W.IsReady()) W.CastOnUnit(target,UsePacket); 
				else
				{
					if (Q.IsReady() && W.IsReady() && E.IsReady() && GetComboDamage(target) > target.Health)
					{
						if (Q.IsReady()) Q.CastOnUnit(target,UsePacket);
						else if (R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
						else if (W.IsReady()) W.CastOnUnit(target,UsePacket);
						else if (E.IsReady()) E.CastOnUnit(target,UsePacket);
					}
					else if (Math.Abs(myHero.PercentCooldownMod) >= 0.2)
					{
						if (CountEnemyInRange(target,300) > 1)
						{
							if (LastCast == "Q")
							{
								if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
								if (R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
								if (!R.IsReady()) W.CastOnUnit(target ,UsePacket);
								if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(target ,UsePacket);
							}
							else Q.CastOnUnit(target,UsePacket);
						}
						else
						{
							if (LastCast == "Q")
							{
								if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
								if (W.IsReady()) W.CastOnUnit(target ,UsePacket);
								if (!W.IsReady()) E.CastOnUnit(target ,UsePacket);
								if (!W.IsReady() && !E.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
							}
							else 
								if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
						}
					}
					else
					{
						if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
						else if (R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
						else if (E.IsReady()) E.CastOnUnit(target ,UsePacket);
						else if (W.IsReady()) W.CastOnUnit(target ,UsePacket);
					}
				}
        	}
        }
        
        private static void ComboBurst()
        {
        	var UseR = Config.Item("UseR").GetValue<bool>();
        	var UseIgnite = Config.Item("UseIgnite").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	if (target == null) return;
        	
        	if (UseIgnite && IgniteKillable(target))
        		if (IgniteSlot != SpellSlot.Unknown && myHero.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && myHero.Distance(target) <= 600)
					myHero.SummonerSpellbook.CastSpell(IgniteSlot, target); 
        	
        	if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target ,UsePacket);
        	else
        	{
        		if (Q.IsKillable(target) && Q.IsReady()) Q.CastOnUnit(target,UsePacket);
        		else if (E.IsKillable(target) && E.IsReady()) E.CastOnUnit(target,UsePacket);
        		else if (W.IsKillable(target) && W.IsReady()) W.CastOnUnit(target,UsePacket); 
        		else if (myHero.Distance(target) >= 575 && !IsFacing(target,myHero) && W.IsReady()) W.CastOnUnit(target,UsePacket);     		   
        		else
        		{
        			if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
					else if (R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
					else if (E.IsReady()) E.CastOnUnit(target ,UsePacket);
					else if (W.IsReady()) W.CastOnUnit(target ,UsePacket);
        		}
        	}		
        }
        
        private static void ComboLong()
        {
        	var UseR = Config.Item("UseR").GetValue<bool>();
        	var UseIgnite = Config.Item("UseIgnite").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	if (target == null) return;
        	
        	if (UseIgnite && IgniteKillable(target))
        		if (IgniteSlot != SpellSlot.Unknown && myHero.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && myHero.Distance(target) <= 600)
					myHero.SummonerSpellbook.CastSpell(IgniteSlot, target); 
        	
        	if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target ,UsePacket);
        	else
        	{
        		if (Q.IsKillable(target) && Q.IsReady()) Q.CastOnUnit(target,UsePacket);
        		else if (E.IsKillable(target) && E.IsReady()) E.CastOnUnit(target,UsePacket);  
        		else if (W.IsKillable(target) && W.IsReady()) W.CastOnUnit(target,UsePacket); 
        		else if (myHero.Distance(target) >= 575 && !IsFacing(target,myHero) && W.IsReady()) W.CastOnUnit(target,UsePacket);     		    
        		else
        		{
        			if (CountEnemyInRange(target,300) > 1)
					{
						if (LastCast == "Q")
						{
							if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
							if (R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
							if (!R.IsReady()) W.CastOnUnit(target ,UsePacket);
							if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(target ,UsePacket);
						}
						else Q.CastOnUnit(target,UsePacket);
					}
        			else
        			{
        				if (LastCast == "Q")
        				{
        					if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
        					if (W.IsReady()) W.CastOnUnit(target ,UsePacket);
        					if (!W.IsReady()) E.CastOnUnit(target ,UsePacket);
        					if (!W.IsReady() && !E.IsReady() && R.IsReady() && UseR) R.CastOnUnit(myHero,UsePacket);
        				}
        				else
        					if (Q.IsReady()) Q.CastOnUnit(target ,UsePacket);
        			}
        		}
        	}
        }
        
       	private static void Harass()
        {
        	var UseQ = Config.Item("HQ").GetValue<bool>();
        	var UseW = Config.Item("HW").GetValue<bool>();
        	var UseE = Config.Item("HE").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	if (myHero.Distance(target) <= 625 )
        	{
        		if (UseQ && Q.IsReady()) Q.CastOnUnit(target,UsePacket);
        		if (UseW && W.IsReady()) W.CastOnUnit(target,UsePacket);
        		if (UseE && E.IsReady()) E.CastOnUnit(target,UsePacket);
        	}     	
        }
        
        private static void Farm()
        {
        	var UseQ = Config.Item("FQ").GetValue<bool>();
        	var UseW = Config.Item("FW").GetValue<bool>();
            var UseE = Config.Item("FE").GetValue<bool>();
            var UseR = Config.Item("FR").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	var allMinions = MinionManager.GetMinions(myHero.ServerPosition, Q.Range,MinionTypes.All,MinionTeam.All, MinionOrderTypes.MaxHealth);
        	if (Config.Item("FreezeActive").GetValue<KeyBind>().Active)
        	{
        		if (UseQ && Q.IsReady())
				{
        			foreach (var minion in allMinions)
					{
        				if (minion.IsValidTarget() && Q.IsKillable(minion))
						{
							Q.CastOnUnit(minion,UsePacket);
							return;
						}
					}
                }
                else if (UseR && R.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            R.CastOnUnit(minion, UsePacket);
                            return;
                        }
                    }
                }
        		else if (UseW && W.IsReady())
				{
					foreach (var minion in allMinions)
					{
						if (minion.IsValidTarget(W.Range) && W.IsKillable(minion))
						{
							W.CastOnUnit(minion,UsePacket);
							return;
						}
					}
				}
				else if (UseE && E.IsReady())
				{
					foreach (var minion in allMinions)
					{
						if (minion.IsValidTarget(E.Range) && E.IsKillable(minion))
						{
							E.CastOnUnit(minion,UsePacket);
							return;
						}
					}
				}
        	}
        	else if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
        	{
        		foreach (var minion in allMinions)
				{
        			if (UseQ && Q.IsReady()) Q.CastOnUnit(minion,UsePacket);			
					if (UseW && W.IsReady()) W.CastOnUnit(minion,UsePacket);		
					if (UseE && E.IsReady()) E.CastOnUnit(minion,UsePacket);					
				}
        	}
        }
        
        private static void JungleFarm()
        {
        	var UseQ = Config.Item("JQ").GetValue<bool>();
        	var UseW = Config.Item("JW").GetValue<bool>();
            var UseE = Config.Item("JE").GetValue<bool>();
            var UseR = Config.Item("JR").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	var jungminions = MinionManager.GetMinions(myHero.ServerPosition, Q.Range,MinionTypes.All,MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
			if (jungminions.Count > 0)
			{
                var minion = jungminions[0];
                if (UseQ && Q.IsReady()) Q.CastOnUnit(minion, UsePacket);
                if (UseR && R.IsReady()) R.CastOnUnit(minion, UsePacket);
				if (UseW && W.IsReady()) W.CastOnUnit(minion,UsePacket);
				if (UseE && E.IsReady()) E.CastOnUnit(minion,UsePacket);
			}
        }
        
        private static void KillSteal()
        {
        	var AutoIgnite = Config.Item("AutoIgnite").GetValue<bool>();
        	var KillSteal = Config.Item("KillSteal").GetValue<bool>();
        	var UsePacket = Config.Item("UsePacket").GetValue<bool>();
        	if (AutoIgnite && IgniteSlot != SpellSlot.Unknown && myHero.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
        	{
        		foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => myHero.Distance(enemy) <= 600 && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead && IgniteKillable(enemy)))
					myHero.SummonerSpellbook.CastSpell(IgniteSlot, enemy);        				
        	}
        	if (KillSteal & (Q.IsReady() || W.IsReady() || E.IsReady()))
        	{
        		foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => myHero.Distance(enemy) <= Q.Range && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead))
        		{
        			if (Q.IsReady() && Q.IsKillable(target)) Q.CastOnUnit(enemy,UsePacket);
        			if (W.IsReady() && W.IsKillable(target)) W.CastOnUnit(enemy,UsePacket);
        			if (E.IsReady() && E.IsKillable(target)) E.CastOnUnit(enemy,UsePacket);
        		}
        	
        	}
        }
                
        private static int CountEnemyInRange(Obj_AI_Hero target,float range)
        {
        	int count = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => myHero.Distance3D(enemy,true) <= range*range && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead))
        		count = count + 1 ;
            return count;
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var packetCast = Config.Item("UsePacket").GetValue<bool>();
            var WInterruptSpell = Config.Item("WInterruptSpell").GetValue<bool>();

            if (WInterruptSpell && W.IsReady() && unit.IsValidTarget(W.Range))
            {
                W.CastOnUnit(unit, packetCast);
            }
        }


    }
    class HpBarIndicator
    {

        public static SharpDX.Direct3D9.Device dxDevice = Drawing.Direct3DDevice;
        public static SharpDX.Direct3D9.Line dxLine;

        public Obj_AI_Hero unit { get; set; }

        public float width = 104;

        public float hight = 9;


        public HpBarIndicator()
        {
            dxLine = new Line(dxDevice) { Width = 9 };

            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;

        }


        private static void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
        {
            dxLine.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            dxLine.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            dxLine.OnLostDevice();
        }

        private Vector2 Offset
        {
            get
            {
                if (unit != null)
                {
                    return unit.IsAlly ? new Vector2(34, 9) : new Vector2(10, 20);
                }

                return new Vector2();
            }
        }

        public Vector2 startPosition
        {

            get { return new Vector2(unit.HPBarPosition.X + Offset.X, unit.HPBarPosition.Y + Offset.Y); }
        }


        private float getHpProc(float dmg = 0)
        {
            float health = ((unit.Health - dmg) > 0) ? (unit.Health - dmg) : 0;
            return (health / unit.MaxHealth);
        }

        private Vector2 getHpPosAfterDmg(float dmg)
        {
            float w = getHpProc(dmg) * width;
            return new Vector2(startPosition.X + w, startPosition.Y);
        }

        public void drawDmg(float dmg, System.Drawing.Color color)
        {
            var hpPosNow = getHpPosAfterDmg(0);
            var hpPosAfter = getHpPosAfterDmg(dmg);

            fillHPBar(hpPosNow, hpPosAfter, color);
            //fillHPBar((int)(hpPosNow.X - startPosition.X), (int)(hpPosAfter.X- startPosition.X), color);
        }

        private void fillHPBar(int to, int from, System.Drawing.Color color)
        {
            Vector2 sPos = startPosition;

            for (int i = from; i < to; i++)
            {
                Drawing.DrawLine(sPos.X + i, sPos.Y, sPos.X + i, sPos.Y + 9, 1, color);
            }
        }

        private void fillHPBar(Vector2 from, Vector2 to, System.Drawing.Color color)
        {
            dxLine.Begin();

            dxLine.Draw(new[]
                                    {
                                        new Vector2((int)from.X, (int)from.Y + 4f),
                                        new Vector2( (int)to.X, (int)to.Y + 4f)
                                    }, new ColorBGRA(255, 255, 00, 90));
            // Vector2 sPos = startPosition;
            //Drawing.DrawLine((int)from.X, (int)from.Y + 9f, (int)to.X, (int)to.Y + 9f, 9f, color);

            dxLine.End();
        }

    }
}
