﻿using System;
using EasyConsole;
using GoRogue;
using GoRogue.MapViews;
using GoRogue.MapGeneration.Generators;
using System.Collections.Generic;

namespace GoRogue_PerformanceTests
{
    class MainPage : MenuPage
    {
        public MainPage(Program program)
            : base("Main Menu", program,
                  new Option("Lighting/FOV Tests", LightingFOV),
                  new Option("Dice Notation Tests", DiceNotation),
                  new Option("Line Tests", Line),
                  new Option("Pathing Tests", Pathing),
                  new Option("Quit", Quit))
        { }

        private static void LightingFOV()
        {
            // Doesn't work properly in release mode, only in debug mode.
            /*
            long lightingMem = LightingFOVTests.MemorySingleLightSourceLighting(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
            long fovMem = LightingFOVTests.MemorySingleLightSourceFOV(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
            Console.WriteLine($"Memory for {MAP_WIDTH}x{MAP_HEIGHT}, Radius {LIGHT_RADIUS}:");
            Console.WriteLine($"\tLighting: {lightingMem} bytes");
            Console.WriteLine($"\tFOV     : {fovMem} bytes");
            */

            var timeSingleLighting = LightingFOVTests.TimeForSingleLightSourceLighting(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.SOURCE_TYPE,
                                                                                       Runner.LIGHT_RADIUS, Runner.RADIUS_STRATEGY, Runner.ITERATIONS_FOR_TIMING);
            var timeSingleFOV = LightingFOVTests.TimeForSingleLightSourceFOV(Runner.MAP_WIDTH, Runner.MAP_HEIGHT,
                                                                             Runner.LIGHT_RADIUS, Runner.ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} calculates, single source, {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map, Radius {Runner.LIGHT_RADIUS}:");
            Console.WriteLine($"\tSenseMap: {timeSingleLighting.ToString()}");
            Console.WriteLine($"\tFOV     : {timeSingleFOV.ToString()}");

            Console.WriteLine();
            TestLightingNSource(2);

            Console.WriteLine();
            TestLightingNSource(3);


            Console.WriteLine();
            TestLightingNSource(4);
        }

        private static void DiceNotation()
        {
            var timeForSmallDiceRoll = DiceNotationTests.TimeForDiceRoll("1d6", Runner.ITERATIONS_FOR_TIMING * 10);
            var timeForMediumDiceRoll = DiceNotationTests.TimeForDiceRoll("2d6+3", Runner.ITERATIONS_FOR_TIMING * 10);
            var timeForLargeDiceRoll = DiceNotationTests.TimeForDiceRoll("1d(1d12+4)+3", Runner.ITERATIONS_FOR_TIMING * 10);

            var timeForSmallDiceExpr = DiceNotationTests.TimeForDiceExpression("1d6", Runner.ITERATIONS_FOR_TIMING * 10);
            var timeForMediumDiceExpr = DiceNotationTests.TimeForDiceExpression("2d6+3", Runner.ITERATIONS_FOR_TIMING * 10);
            var timeForLargeDiceExpr = DiceNotationTests.TimeForDiceExpression("1d(1d12+4)+3", Runner.ITERATIONS_FOR_TIMING * 10);

            var timeForKeepRoll = DiceNotationTests.TimeForDiceRoll("5d6k2+3", Runner.ITERATIONS_FOR_TIMING * 10);
            var timeForKeepExpr = DiceNotationTests.TimeForDiceExpression("5d6k2+3", Runner.ITERATIONS_FOR_TIMING * 10);

            Console.WriteLine();
            Console.WriteLine($"Time to roll 1d6, 2d6+3, and 5d6k2+3, 1d(1d12+4)+3 dice {Runner.ITERATIONS_FOR_TIMING * 10} times: ");
            Console.WriteLine("\tRoll Method:");
            Console.WriteLine($"\t\t1d6         : {timeForSmallDiceRoll}");
            Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceRoll}");
            Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepRoll}");
            Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceRoll}");

            Console.WriteLine("\tParse Method:");
            Console.WriteLine($"\t\t1d6         : {timeForSmallDiceExpr}");
            Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceExpr}");
            Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepExpr}");
            Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceExpr}");
            Console.WriteLine();
        }

        private static void Line()
        {
            var timeBres = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.BRESENHAM, Runner.ITERATIONS_FOR_TIMING);
            var timeDDA = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.DDA, Runner.ITERATIONS_FOR_TIMING);
            var timeOrtho = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.ORTHO, Runner.ITERATIONS_FOR_TIMING);

            Console.WriteLine();
            Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} generations of line from {Runner.LINE_START} to {Runner.LINE_END}:");
            Console.WriteLine($"\tBresenham: {timeBres}");
            Console.WriteLine($"\tDDA      : {timeDDA}");
            Console.WriteLine($"\tOrtho    : {timeOrtho}");
        }

        private static void Pathing()
        {
            /* AStar */
            var timeAStar = PathingTests.TimeForAStar(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} paths, on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map:");
            Console.WriteLine($"\tAStar: {timeAStar}");

            /* Single-Goal GoalMap */
            var map = new ArrayMap<bool>(Runner.MAP_WIDTH, Runner.MAP_HEIGHT);
            CellularAutomataGenerator.Generate(map);
            Coord goal = map.RandomPosition(true);

            var timeGoalMap = PathingTests.TimeForSingleSourceGoalMap(map, goal, Runner.ITERATIONS_FOR_TIMING);

            Console.WriteLine();
            Console.WriteLine($"Time to calculate single-source goal map on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map {Runner.ITERATIONS_FOR_TIMING} times:");
            Console.WriteLine($"\tGoal-Map    : {timeGoalMap}");


            /* Multi-Goal GoalMap */
            var goals = new List<Coord>();

            for (int i = 0; i < Runner.NUM_GOALS; i++)
                goals.Add(map.RandomPosition(true));

            var timeMGoalMap = PathingTests.TimeForMultiSourceGoalMap(map, goals, Runner.ITERATIONS_FOR_TIMING);

            Console.WriteLine();
            Console.WriteLine($"Time to calculate multi-source goal map on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map {Runner.ITERATIONS_FOR_TIMING} times:");
            Console.WriteLine($"\tGoal-Map    : {timeMGoalMap}");
        }

        private static void Quit()
        {
            Runner.Quit = true;
        }

        private static void TestLightingNSource(int sources)
        {
            var timeMultipleLighting = LightingFOVTests.TimeForNSourcesLighting(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.LIGHT_RADIUS,
                                                                                Runner.ITERATIONS_FOR_TIMING, sources);
            Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING}, calc fov's, {sources} sources, {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map, Radius {Runner.LIGHT_RADIUS}:");
            Console.WriteLine($"\tLighting: {timeMultipleLighting.ToString()}");
        }
    }
}
