using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using OpenTK;

namespace MejorNX.Maxwell
{
    public unsafe class MaxwellContext
    {
        string Vertex = @"
#version 330 core
layout(location = 0) in vec3 iPosition;
out vec2 uv;
void main()
{
    uv = iPosition.xy;
    uv.y = 1- uv.y;
    gl_Position = vec4((iPosition - 0.5) * 2,1);
}
";

        string Fragment = @"
#version 330 core
out vec4 fragColor;
in vec2 uv;
uniform sampler2D texture0;
void main()
{
    vec4 col = texture(texture0,uv);

    fragColor = vec4(col.b,col.g,col.r,1);
}
";

        public static MaxwellContext MainContext    { get; set; }
        public MaxwellVirtualMemoryManager Vmm      { get; set; }
        public CommandStack CommandStack            { get; set; }

        public Gpu2dEngine _2dEngine                { get; set; }
        public Gpu3dEngine _3dEngine                { get; set; }
        public GpuDmaEngine dmaEngine               { get; set; }

        public MaxwellContext(void* BaseAddress)
        {
            MainContext = this;

            Vmm = new MaxwellVirtualMemoryManager(BaseAddress);

            CommandStack = new CommandStack();

            _2dEngine = new Gpu2dEngine(this);
            _3dEngine = new Gpu3dEngine(this);
            dmaEngine = new GpuDmaEngine(this);

            fb = new uint[1280 * 720];
        }

        uint[] fb;

        bool started = false;
        int VAO;

        int i = 0;

        public void ProcessFrame()
        {
            CommandStack.ExecuteCommands(this);
        }

        public int GetOffset(int x, int y)
        {
            return (x * 1280) + y;
        }
    }

    public class ShaderProgram
    {
        public List<ShaderSource> Shaders { get; private set; }
        public int Program { get; set; }
        public bool Compiled => Program != -1;

        public ShaderProgram()
        {
            Shaders = new List<ShaderSource>();
            Program = -1;
        }

        public void AddShader(ShaderSource source)
        {
            Shaders.Add(source);
        }

        public void RemoveShader(ShaderSource source)
        {
            Shaders.Remove(source);
        }

        public void Compile()
        {
            if (Compiled)
            {
                GL.DeleteProgram(Program);

                Program = -1;
            }

            Program = GL.CreateProgram();

            foreach (ShaderSource source in Shaders)
            {
                source.CompileShader();

                GL.AttachShader(Program, source.Handle);
            }

            GL.LinkProgram(Program);
            GL.ValidateProgram(Program);
        }

        public void Use()
        {
            GL.UseProgram(Program);
        }
    }

    public class ShaderSource
    {
        public int Handle { get; set; }
        public string Source { get; set; }
        public ShaderType Type { get; set; }

        public bool Compiled => Handle != -1;

        public ShaderSource(string Source, ShaderType Type)
        {
            Handle = -1;
            this.Source = Source;
            this.Type = Type;
        }

        public void CompileShader()
        {
            if (Compiled)
            {
                GL.DeleteShader(Handle);

                Handle = -1;
            }

            Handle = GL.CreateShader(Type);

            GL.ShaderSource(Handle, Source);
            GL.CompileShader(Handle);

            string Error = GL.GetShaderInfoLog(Handle);
        }
    }
}
