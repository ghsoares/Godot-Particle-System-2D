# Godot Particle System 2D
This is a modular Particle System 2D for Godot 3.2+ (currently only supports on Mono).

**Attention** This plugin is currently at a very early stage, is not recommended to use in your personal projects as can lead to editor crashes, slowdowns and maybe corrupt the entire project.

The particle system uses modules to determine the particle initialization, update, destruction and visual. The purpose is to have a easy to extend
system and to do so, you can create a custom module with a custom behaviour, the plugin comes with some built-in modules (more will be added on the future):

### Emit options module
Initializes a particle with a lifetime, speed, size, rotation and color. Lifetime, speed, size and rotation can be random.

### Emit rate module
Emits a new particle given a rate/second.

### Update Particle
Important module, updates particle life, position and velocity every frame.

### Size over life
Applies size over the life of the particle provided by a Curve.

### Color over life
Applies color over the life of the particle provided by a Gradient.

### Collision
Simple collision module, detects collision with the world with raycasts. Bounciness control how much the particle can bounce off a surface.

### Draw Batch
Optimized particles drawing module, takes all the particles and draw all of then in a single MultiMesh, all the particles only use one triangle.

The ParticleSystem2D also has some options:
- `Update Mode`, tells how the particle system will be updated, in the _process or _physics_process;
- `Space Mode`, tells the space of the particles, local or world;
- `Max Particles`, tells how much particles are used for the system, no more particles are emitted from this limit;
- `Seed`, tells the seed used for the random methods inside the modules;
- `Emitting`, tells if the system is emitting or not;
- `Emit On Start`, tells if the system starts to emit when the scene is loaded;
- `Gravity`, main gravity vector of the system;
- `Time Scale`, multiplies the delta value used to update the particles, can speed up or slow down the particles;
- `On Editor Draw FPS`, tells the FPS of the system drawing in the editor, used to prevent slow downs;
- `Debug Mode`, useful to tell the modules to draw or print something.

The ParticleSystem2D also has a public Dictionary `modulesData`, is not recommended to change this dictionary as it is used to serialize and save the modules data with the scene that is loaded with the scene.

## How to create a new module
This is a template that can be used to easily create a new module:
```csharp
using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    /*
    Put the path of the module that shows in the "Add Module" context menu
    */
    [ParticleModulePath("Custom/Custom Module")]
    public class ParticleSystem2DCustomModule : ParticleSystem2DModule
    {
        // This function is called when this module is added or the game starts
        public override void InitModule()
        {
            GD.Print("Custom Module Initialized!");
        }

        // Override this function to draw the module interface with the input fields
        public override void DrawInterface(Control parent)
        {
            
        }

        // Override this function to provide a data to be saved with the scene
        public override Godot.Collections.Dictionary SerializeModule()
        {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            
            return data;
        }

        // This function is called to load the module from a saved data
        public override void UnSerializeModule(Godot.Collections.Dictionary data)
        {
            
        }

        // Override this function to provide the module name displayed in the modules list of a ParticleSystem2D
        public override string GetModuleName()
        {
            return "Custom Module";
        }

        // This function is called every frame to update the module
        public override void UpdateModule(float delta)
        {

        }

        // Override this function to draw things like gizmos, etc.
        public override void DrawModule()
        {

        }
    }
}
```
Your module still can't interact with the particles, to do so, you must implement these interfaces:
### IParticleInitializable
Implements `void InitParticle(ref Particle particle, ParticleSystem2D.EmitParams emitparams)`
Use this interface if you want to change the particle initialization behaviour.

### IParticleUpdateable
Implements `void UpdateParticle(ref Particle particle, float delta)`
Use this interface if you want to change the particle update behaviour. `delta` is multiplied by the `ParticleSystem2D.timeScale`.

### IParticleDestroyable
Implements `void DestroyParticle(ref Particle particle)`
Use this interface if you want to change the particle detroy behaviour. Useful for effects like fireworks.

### IParticleDrawable
Implements `void DrawParticle(Particle particle)`
Use this interface if you want to draw a single particle at a time, can cause slowdown.

### IParticleBatchDrawable
Implements `void DrawBatch(Particle[] particles)`
Use this interface if you want to draw all the particles at once.
