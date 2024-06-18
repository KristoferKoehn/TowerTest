using Godot;
using Godot.Collections;
using System;

public partial class SettingsManager : Node
{

    private static SettingsManager instance;

    public Dictionary<string, float> SettingsDictionary = new();

    private SettingsManager() {
        //load in settings file OR generate settings file

        string SettingsText = Load();
        if (SettingsText != null)
        {
            SettingsDictionary = (Dictionary<string, float>)Json.ParseString(SettingsText);
        } else
        {

            GD.Print("MAKING NEW FILE");
            SettingsDictionary = new Dictionary<string, float> {
                { "Resolution" , 648f },
                { "AspectRatio" , 16/9f},
                { "ResolutionScale", 0.5f},
                { "VSync", 0f}, //0 disabled, 1 enabled, 2 adaptive, 3 mailbox
                { "Borderless", 0f}, //0 disabled, 1 enabled
                { "Fullscreen", 0f}, //0 disabled, 1 enabled
                { "MSAA", 0f},
                { "FSR 2.2", 2f},

                //there's a shitload of anti-aliasing options that we can expose for the user
                //since this isn't a graphically complex game though idk if this will be of any actual value


                //these are not implemented yet
                { "CameraProjection", 1f}, //1 is projection, 0 is orthographic
                { "FOV", 60f },
                { "Size", 25f}, //size is for orthographic zoom
                { "Shadows", 1f}, //1 is on 0 is off
                { "Glow", 1f },
                { "SDGFI", 0f},

            };

            Save(Json.Stringify(SettingsDictionary));

        }
    }

    public void ApplySettings()
    {
        foreach(string key in SettingsDictionary.Keys)
        {
            GD.Print($"{key} = {SettingsDictionary[key]}");
        }

        //camera settings are applied camera-side

        if ((int)SettingsDictionary["Borderless"] != 0)
        {
            SceneSwitcher.root.GetWindow().Borderless = true;
        } else
        {
            SceneSwitcher.root.GetWindow().Borderless = false;
        }

        

        if ((int)SettingsDictionary["Fullscreen"] == 0)
        {
            GetWindow().Mode = Window.ModeEnum.Windowed;
            //aspect ratio
            //and resolution
            Vector2I vector2I = new Vector2I((int)(SettingsDictionary["Resolution"] * SettingsDictionary["AspectRatio"]), (int)(SettingsDictionary["Resolution"]));
            GetWindow().Size = vector2I;
        } else
        {
            GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
        }


        GetViewport().Scaling3DScale = SettingsDictionary["ResolutionScale"];

        int vs = (int)SettingsDictionary["VSync"];
        DisplayServer.WindowSetVsyncMode((DisplayServer.VSyncMode)vs);


        if ((Viewport.Msaa)(int)SettingsDictionary["MSAA"] != Viewport.Msaa.Disabled)
        {
            GetViewport().Msaa3D = (Viewport.Msaa)(int)SettingsDictionary["MSAA"];
        } else if ((Viewport.Scaling3DModeEnum)(int)SettingsDictionary["FSR 2.2"] != Viewport.Scaling3DModeEnum.Bilinear)
        {
            long fsr = (long)SettingsDictionary["FSR 2.2"];
            GetViewport().Scaling3DMode = (Viewport.Scaling3DModeEnum)fsr;
        }
    }

    public static SettingsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new SettingsManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "SettingsManager";
        }
        return instance;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        ApplySettings();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
	}

    public void Save(string content)
    {
        using var file = FileAccess.Open("user://settings.dat", FileAccess.ModeFlags.Write);
        file.StoreString(content);
    }

    public string Load()
    {
        using var file = FileAccess.Open("user://settings.dat", FileAccess.ModeFlags.Read);
        if(file == null)
        {
            return null;
        }
        string content = file.GetAsText();
        return content;
    }
}
