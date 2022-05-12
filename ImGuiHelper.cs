using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET;
using SIMD = System.Numerics;

namespace CSharpPathTracer
{
	class ImGuiHelper
	{
		/// <summary>
		/// Static representation of a vertex to match ImGui vertices
		/// </summary>
		private static class ImGuiVertex
		{
			public static int Size { get; private set; }
			public static VertexDeclaration Format { get; private set; }

			static ImGuiVertex()
			{
				unsafe { Size = sizeof(ImDrawVert); }

				Format = new VertexDeclaration(
					Size,
					new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
					new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
					new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0));
			}
		}

		/// <summary>
		/// Represents the data for a single buffer used by the UI
		/// </summary>
		/// <typeparam name="T">Type of buffer (vertex or index)</typeparam>
		private struct BufferPackage<T> where T : GraphicsResource
		{
			public int Size;
			public byte[] Data;
			public T Buffer;

			public void Resize(int newSize, GraphicsDevice device)
			{
				// Don't resize smaller
				if (newSize <= Size)
					return;

				// Reset resource
				Buffer?.Dispose();
				Buffer = null;

				// Update sizes
				Size = newSize;

				// Handle buffer resizing based on type
				if (typeof(T) == typeof(VertexBuffer))
				{
					Data = new byte[Size * ImGuiVertex.Size];
					Buffer = (T)(GraphicsResource)(new VertexBuffer(device, ImGuiVertex.Format, Size, BufferUsage.None));
				}
				else if (typeof(T) == typeof(IndexBuffer))
				{
					Data = new byte[Size * sizeof(UInt16)];
					Buffer = (T)(GraphicsResource)(new IndexBuffer(device, IndexElementSize.SixteenBits, Size, BufferUsage.None));
				}
			}
		}

		// Reference to the game in progress
		private Game game;

		// Need to track the previous mouse wheel value to calc offset
		private int totalScrollWheel;

		// Drawing related resources
		BufferPackage<VertexBuffer> vertBuffer;
		BufferPackage<IndexBuffer> indexBuffer;
		BasicEffect effect;
		Texture2D fontTexture;
		RasterizerState imguiRasterizer;

		/// <summary>
		/// Creates the UI rendering helper object
		/// </summary>
		/// <param name="game">The game associated with this UI</param>
		public ImGuiHelper(Game game)
		{
			this.game = game;
			game.Window.TextInput += Window_TextInput;

			totalScrollWheel = 0;

			effect = new BasicEffect(game.GraphicsDevice);
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = true;
			effect.World = Matrix.Identity;
			effect.View = Matrix.Identity;
			effect.Projection = Matrix.CreateOrthographic(
				game.GraphicsDevice.PresentationParameters.BackBufferWidth,
				game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				0.0f,
				1.0f);

			imguiRasterizer = new RasterizerState();
			imguiRasterizer.CullMode = CullMode.None;
			imguiRasterizer.FillMode = FillMode.Solid;
			imguiRasterizer.ScissorTestEnable = true;

			// Initialize ImGui
			IntPtr context = ImGui.CreateContext();
			ImGui.SetCurrentContext(context);

			// Create the font texture
			CreateFontTexture();
		}

		/// <summary>
		/// Text input event to hook up input to ImGui
		/// </summary>
		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			ImGui.GetIO().AddInputCharacter(e.Character);
		}

		/// <summary>
		/// Creates and copies the ImGui font texture to a MonoGame font 
		/// </summary>
		private void CreateFontTexture()
		{
			ImGuiIOPtr io = ImGui.GetIO();
			io.Fonts.AddFontDefault();

			unsafe
			{
				// Grab ImGui's built-in font texture data
				io.Fonts.GetTexDataAsRGBA32(
					out byte* imguiFontTextureData,
					out int width,
					out int height,
					out int bytesPerPixel);

				// Copy to a byte array
				byte[] pixels = new byte[width * height * bytesPerPixel];
				System.Runtime.InteropServices.Marshal.Copy(
					new IntPtr(imguiFontTextureData),
					pixels,
					0,
					pixels.Length);

				// Create the actual MonoGame texture
				fontTexture = new Texture2D(game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
				fontTexture.SetData<byte>(pixels);

				// Assume the font texture is always the first
				io.Fonts.SetTexID(new IntPtr(0));
				io.Fonts.ClearTexData(); // Don't need CPU texture anymore
			}
		}

		/// <summary>
		/// Prepares Imgui before the UI is built for the frame
		/// </summary>
		/// <param name="gt">Game time info</param>
		public void PreUpdate(GameTime gt)
		{
			ImGuiIOPtr io = ImGui.GetIO();
			MouseState ms = Mouse.GetState();
			KeyboardState kb = Keyboard.GetState();

			// Set all input data
			io.DeltaTime = (float)gt.ElapsedGameTime.TotalSeconds;
			io.DisplaySize.X = game.GraphicsDevice.PresentationParameters.BackBufferWidth;
			io.DisplaySize.Y = game.GraphicsDevice.PresentationParameters.BackBufferHeight;
			io.DisplayFramebufferScale.X = 1.0f;
			io.DisplayFramebufferScale.Y = 1.0f;

			io.MousePos.X = ms.X;
			io.MousePos.Y = ms.Y;
			io.MouseDown[0] = ms.LeftButton == ButtonState.Pressed;
			io.MouseDown[1] = ms.RightButton == ButtonState.Pressed;
			io.MouseDown[2] = ms.MiddleButton == ButtonState.Pressed;
			io.MouseWheel = ms.ScrollWheelValue - totalScrollWheel;
			totalScrollWheel = ms.ScrollWheelValue;

			for (int i = 0; i < 256; i++)
			{
				io.KeysDown[i] = kb.IsKeyDown((Keys)i);
			}
			io.KeyAlt = kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt);
			io.KeyCtrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
			io.KeyShift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
			io.KeySuper = kb.IsKeyDown(Keys.LeftWindows) || kb.IsKeyDown(Keys.RightWindows);

			// Set up the new frame
			ImGui.NewFrame();
		}

		/// <summary>
		/// Draws the UI
		/// </summary>
		public void Draw()
		{
			// Get the device
			GraphicsDevice device = game.GraphicsDevice;

			// Render the final UI for the frame
			ImGui.Render();

			// Grab previous states
			Viewport prevVP = device.Viewport;
			Rectangle prevScissor = device.ScissorRectangle;
			BlendState prevBlend = device.BlendState;
			RasterizerState prevRast = device.RasterizerState;
			DepthStencilState prevDepth = device.DepthStencilState;

			// Set up new states
			device.Viewport = new Viewport(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
			device.BlendFactor = Color.White;
			device.BlendState = BlendState.NonPremultiplied;
			device.DepthStencilState = DepthStencilState.DepthRead;
			device.RasterizerState = imguiRasterizer;

			// Scale coords if necessary
			ImDrawDataPtr drawData = ImGui.GetDrawData();
			drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

			// Update projection if display size has changed
			effect.Projection = Matrix.CreateOrthographicOffCenter(
						0.0f,
						ImGui.GetIO().DisplaySize.X,
						ImGui.GetIO().DisplaySize.Y,
						0.0f,
						-1.0f,
						1.0f);

			// Fill buffers and draw
			UpdateBuffers(drawData);
			DrawUI(drawData);

			// Restore state
			device.Viewport = prevVP;
			device.BlendState = prevBlend;
			device.DepthStencilState = prevDepth;
			device.RasterizerState = prevRast;
		}

		/// <summary>
		/// Updates vert and index buffers to prepare for drawing
		/// </summary>
		/// <param name="drawData">Current draw data details</param>
		private void UpdateBuffers(ImDrawDataPtr drawData)
		{
			// Attempt resizes if necessary
			vertBuffer.Resize(drawData.TotalVtxCount, game.GraphicsDevice);
			indexBuffer.Resize(drawData.TotalIdxCount, game.GraphicsDevice);

			// Loop through command lists and reserve enough size
			int vertOffset = 0;
			int indexOffset = 0;
			for (int i = 0; i < drawData.CmdListsCount; i++)
			{
				ImDrawListPtr commandList = drawData.CmdListsRange[i];

				unsafe
				{
					// Copy vertex data
					fixed (void* copyDest = &vertBuffer.Data[vertOffset * ImGuiVertex.Size])
					{
						Buffer.MemoryCopy(
							(void*)commandList.VtxBuffer.Data,
							copyDest,
							vertBuffer.Data.Length,
							commandList.VtxBuffer.Size * ImGuiVertex.Size);
					}

					// Copy index data
					fixed (void* copyDest = &indexBuffer.Data[indexOffset * sizeof(UInt16)])
					{
						Buffer.MemoryCopy(
							(void*)commandList.IdxBuffer.Data,
							copyDest,
							indexBuffer.Data.Length,
							commandList.IdxBuffer.Size * sizeof(UInt16));
					}
				}

				// Increment offsets
				vertOffset += commandList.VtxBuffer.Size;
				indexOffset += commandList.IdxBuffer.Size;
			}

			// Copy data to GPU
			vertBuffer.Buffer.SetData(vertBuffer.Data, 0, drawData.TotalVtxCount * ImGuiVertex.Size);
			indexBuffer.Buffer.SetData(indexBuffer.Data, 0, drawData.TotalIdxCount * sizeof(UInt16));
		}

		/// <summary>
		/// Actually draws the UI to the screen
		/// </summary>
		/// <param name="drawData">Current draw data</param>
		private void DrawUI(ImDrawDataPtr drawData)
		{
			// Set buffers
			game.GraphicsDevice.SetVertexBuffer(vertBuffer.Buffer);
			game.GraphicsDevice.Indices = indexBuffer.Buffer;

			// Track offsets
			int vertOffset = 0;
			int indexOffset = 0;

			// Loop through command lists
			for (int i = 0; i < drawData.CmdListsCount; i++)
			{
				// Grab this command list and loop through commands
				ImDrawListPtr commandList = drawData.CmdListsRange[i];
				for (int c = 0; c < commandList.CmdBuffer.Size; c++)
				{
					// Grab this command
					ImDrawCmdPtr drawCommand = commandList.CmdBuffer[c];

					// Anything to do?
					if (drawCommand.ElemCount == 0)
						continue;

					// Set up scissor rect
					SIMD.Vector4 clip = drawCommand.ClipRect;
					game.GraphicsDevice.ScissorRectangle = new Rectangle(
						(int)clip.X,
						(int)clip.Y,
						(int)(clip.Z - clip.X),
						(int)(clip.W - clip.Y));

					// Update effect's texture
					effect.Texture = fontTexture; // TODO: Adjust based on required texture!

					// Loop through passes and draw
					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						game.GraphicsDevice.DrawIndexedPrimitives(
							PrimitiveType.TriangleList,
							(int)drawCommand.VtxOffset + vertOffset,
							(int)drawCommand.IdxOffset + indexOffset,
							(int)drawCommand.ElemCount / 3);
					}
				}

				// Increment offsets
				vertOffset += commandList.VtxBuffer.Size;
				indexOffset += commandList.IdxBuffer.Size;
			}
		}

	}
}
