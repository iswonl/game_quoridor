using System;
namespace quoridor
{
    public class Minimax
    {
        private Random rand = new Random();

        //public Minimax(char maximizerPawnName)
        //{
        //    MaximizerPawnName = maximizerPawnName;
        //}

        private Dictionary<char, List<Field>> GetPaths(Position position)
        {
            Dictionary<char, List<Field>> paths = new();
            for (int i = 0;i < position.PawnsOnBoard.Count;i ++)
            {
                Pawn pawn = position.PawnsOnBoard[i];
                paths[pawn.Name] = new Position(position).GetShortestPathFor(pawn.Name);
            }
            return paths;
        }

        private int BestWeight(Dictionary<char, List<Field>> paths,Position position)
        {
            int weight = int.MaxValue;
            foreach (Pawn pawn in position.PawnsOnBoard)
            {
                if (pawn.Name != position.currentPlayer.PawnName)
                {
                    weight = Math.Min(weight,paths[pawn.Name][0].Length);
                }
            }
            return weight;
        }

        private int GetWeight(Position position)
        {
            Dictionary<char, List<Field>> currentPaths = GetPaths(position);
            int weight = BestWeight(currentPaths, position);
            int botWeight = currentPaths[position.currentPlayer.PawnName][0].Length;
            return GetWeight(botWeight, weight);
        }

        private int GetWeight(int botWeight,int weight)
        {
            if (weight == 0) return 1;
            if (botWeight == 1) return 1;
            return botWeight - weight;
        }

        public Wall GetBestWall(Position position)
        {
            if (position.currentPlayer.WallsLeft == 0) return null;
            Wall bestWall = null;
            int bestWeight = int.MaxValue;
            Dictionary<char, List<Field>> paths = GetPaths(position);
            foreach (Pawn pawn in position.PawnsOnBoard)
            {
                if (pawn.Name != position.currentPlayer.PawnName)
                {
                    List<Field> path = paths[pawn.Name];
                    List<Wall> walls = new();
                    foreach (Field field in path)
                    {
                        Position currentPosition = new Position(position);
                        foreach (char type in new char[]{ 'v','h'})
                        foreach (int row in new int[] { 0, -1})
                        foreach (int col in new int[] { 0, -1}){
                            Wall currentWall = new Wall(type, field.Pawn.Row + row, field.Pawn.Col + col);
                            if (!QuoridorEngine.ContainsWall(currentWall, walls))
                            {
                                if (currentPosition.SetWall(currentWall.Orientation, currentWall.Row, currentWall.Col))
                                {
                                    int currentWeight = -GetWeight(currentPosition);
                                    if (currentWeight < bestWeight)
                                    {
                                        bestWeight = currentWeight;
                                        bestWall = currentWall;
                                    }else if (currentWeight == bestWeight && rand.Next(2) == 1)
                                                {
                                                    bestWeight = currentWeight;
                                                    bestWall = currentWall;
                                                }
                                            currentPosition = new Position(position);
                                }
                             }
                             else
                             {
                                 walls.Add(currentWall);
                             }
                        }
                    }
                }
            }
            return bestWall;
        }

        public Pawn getBestPawn(QuoridorEngine quoridorEngine)
        {
            List<Field> path = quoridorEngine.GetShortestPathFor(quoridorEngine.currentPlayer.PawnName);
            if (path.Count == 1) return path[0].Pawn;
            return path[path.Count - 2].Pawn;
        }





        public int PerformBestAction(QuoridorEngine quoridorEngine, int depth)
        {
            Position pawnPosition = new Position(quoridorEngine);
            Pawn pawn = getBestPawn(pawnPosition);
            pawnPosition.MovePiece(pawn.Name, pawn.Col, pawn.Row);
            int pawnWeight = GetWeight(pawnPosition);
            Position wallPosition = new Position(quoridorEngine);
            Wall wall = GetBestWall(wallPosition);
            if (wall != null)wallPosition.SetWall(wall.Orientation, wall.Row, wall.Col);
            int wallWeight = GetWeight(wallPosition);
            if (wall != null) wallWeight = 20;

            if (depth > 0)
            {
                pawnWeight = PerformBestAction(pawnPosition, depth - 1);
                if(wall != null)wallWeight = PerformBestAction(wallPosition, depth - 1);
            }
            if (pawnWeight <= wallWeight || wall == null)
            {
                quoridorEngine.MovePiece(pawn.Name, pawn.Col, pawn.Row);
                return pawnWeight;
            }
            else
            {
                quoridorEngine.SetWall(wall.Orientation, wall.Row, wall.Col);
                return wallWeight;
            }
        }


        public void GetMove(QuoridorEngine quoridorEngine)
        {
            if (!quoridorEngine.IsGameEnded())
            {
                PerformBestAction(quoridorEngine, 2);
            }
            else
            {
                Console.ReadKey();
            }
            GC.Collect();
            //Console.WriteLine($"move({pawn.Name} {pawn.Col} {pawn.Row})");
        }
    }
}

