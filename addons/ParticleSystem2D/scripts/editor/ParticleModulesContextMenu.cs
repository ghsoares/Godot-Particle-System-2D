using Godot;
using System.Linq;
using System.Collections.Generic;

namespace ParticleSystem2DPlugin
{
	[Tool]
	public class ParticleModulesContextMenu : Control
	{
		private VBoxContainer list { get; set; }
		private Button close { get; set; }

		public List<string> particleModules { get; set; }

		[Signal]
		delegate void Close();
		[Signal]
		delegate void ModuleChoosed(string typeName);

		public override void _EnterTree()
		{
			close = GetNode<Button>("Container/VBox/Close");
			list = GetNode<VBoxContainer>("Container/VBox/Scroll/VBox");

			foreach (Node c in list.GetChildren())
			{
				c.QueueFree();
			}

			System.Type[] modules = typeof(ParticleSystem2DModule).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ParticleSystem2DModule))).ToArray();

			foreach (System.Type t in modules)
			{
				ParticleModulePathAttribute pathAttribute = t.GetCustomAttributes(
					typeof(ParticleModulePathAttribute), true
				).FirstOrDefault() as ParticleModulePathAttribute;

				if (pathAttribute == null) continue;

				string path = pathAttribute.path;
				string[] splitted = path.Split("/");
				int margin = 0;
				Control parent = list;
				for (int i = 0; i < splitted.Length; i++)
				{
					string p = splitted[i];
					bool hasItem = parent.HasNode(p);
					if (!hasItem)
					{
						VBoxContainer item = new VBoxContainer();
						item.Name = p;
						item.RectMinSize = new Vector2(0, 16);
						if (i < splitted.Length - 1)
						{
							Label l = new Label();
							l.Text = new string(' ', margin) + p;
							item.AddChild(l);
						}
						else
						{
							Button b = new Button();
							b.Text = new string(' ', margin) + p;
							b.Align = Button.TextAlign.Left;
							if (particleModules.Contains(t.ToString())) b.Disabled = true;
							b.Connect("pressed", this, "OnModulePressed", new Godot.Collections.Array { t.ToString() });
							item.AddChild(b);
						}
						parent.AddChild(item);
						parent = item;
					} else {
						parent = parent.GetNode<Control>(p);
					}
					margin += 4;
				}
			}

			close.Connect("pressed", this, "OnClosePressed");
		}

		public void OnModulePressed(string moduleTypeName)
		{
			EmitSignal("ModuleChoosed", moduleTypeName);
			EmitSignal("Close");
			QueueFree();
		}

		public void OnClosePressed()
		{
			EmitSignal("Close");
			QueueFree();
		}
	}
}
