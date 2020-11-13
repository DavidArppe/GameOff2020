﻿namespace Terra.Graph.Generators {
	public abstract class AbsFractalNoiseNode: AbsGeneratorNode {
		[Input] public float Frequency = 1f;
		[Input] public float Lacunarity = 2.17f;
		[Input] public int OctaveCount = 6;

        [Input] public bool Tiling = true;
        [Input] public float Period = 1f;

        protected const string MENU_PARENT_NAME = "Terrain/Noise/";
	}
}