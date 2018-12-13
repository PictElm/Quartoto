using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartzoto {

    class Program {

        const int SIZE = 4;
        const int TILE_SIZE = 3;
        const int EMPTY = -1;

        const ConsoleColor MAIN_BG = ConsoleColor.Black;
        const ConsoleColor TILE_BG = ConsoleColor.Black;

        const ConsoleColor MAIN_FG = ConsoleColor.Gray;
        const ConsoleColor COLOR_1 = ConsoleColor.Blue;
        const ConsoleColor COLOR_2 = ConsoleColor.Red;
        const ConsoleColor COLOR_SEL = ConsoleColor.Green;

        const int FLAG_IS_COL1 = 1;
        const int FLAG_IS_SQRE = 2;
        const int FLAG_IS_TALL = 4;
        const int FLAG_IS_HOLE = 8;

        static int[,] grid;
        static int[] piecesLefts;

        static String currentPlayer;

        static void Main(string[] args) {
            Initialize(); // Initialise.
            String winner = PlayGame("Alexa", "Patrick"); // Fait une partie entre "Alexa" et "Patrick".
            Println(winner + " wins!", MAIN_BG, ConsoleColor.Yellow);
        }

        static void Initialize() {
            // Initialise la liste des pièces disponibles (la 'pioche').
            piecesLefts = new int[SIZE * SIZE];
            for (int k = 0; k < SIZE * SIZE; piecesLefts[k] = k++)
                ;

            // Initialise la grille à `0xFF..F` (-1).
            grid = new int[SIZE, SIZE];
            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    grid[i, j] = EMPTY;
        }

        static void Print(Object c, ConsoleColor bg=MAIN_BG, ConsoleColor fg=MAIN_FG) {
            if (Console.BackgroundColor != bg)
                Console.BackgroundColor = bg;

            if (Console.ForegroundColor != fg)
                Console.ForegroundColor = fg;
            
            Console.Write(c);
        }

        static void Println(Object c=null, ConsoleColor bg=MAIN_BG, ConsoleColor fg=MAIN_FG) {
            if (c == null)
                Console.WriteLine();
            else
                Print(c.ToString() + "\n", bg, fg);
        }

        static String[] DetailPiece(int piece) {
            if (piece == EMPTY) {
                String[] r = new String[TILE_SIZE];
                for (int k = 0; k < TILE_SIZE; r[k++] = new String(' ', TILE_SIZE))
                    ;
                return r;
            }

            bool sqre = (piece & FLAG_IS_SQRE) != 0;
            bool tall = (piece & FLAG_IS_TALL) != 0;
            bool hole = (piece & FLAG_IS_HOLE) != 0;

            String gap = hole ? (tall ? " V_" : "  V") : (tall ? "_ _" : " __");
            String shapeR = sqre ? "[" : "(";
            String shapeL = sqre ? "]" : ")";

            return new String[] { " " + gap[0] + " ",
                                  tall ? "" + shapeR + gap[1] + shapeL : " " + gap[1] + " ",
                                  "" + shapeR + gap[2] + shapeL };
        }

        static void DisplayGrid(int pieceToPlace=EMPTY, params int[] highlighted) {
            if (pieceToPlace != EMPTY) {
            }

            String[,][] lines = new String[SIZE, SIZE][];
            ConsoleColor[,] colors = new ConsoleColor[SIZE, SIZE];

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++) {
                    lines[i, j] = DetailPiece(grid[i, j]);
                    if (highlighted.Length == 2 && highlighted[0] == i && highlighted[1] == j)
                        colors[i, j] = COLOR_SEL;
                    else
                        colors[i, j] = grid[i, j] == EMPTY ? MAIN_FG : (grid[i, j] & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;
                }

            String[] lineToPlace = DetailPiece(pieceToPlace);
            ConsoleColor colorToPlace = (pieceToPlace & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;

            Println("  " + new String('-', SIZE * (TILE_SIZE + 1) + 1));

            for (int j = 0; j < SIZE; j++) {
                for (int k = 0; k < TILE_SIZE; k++) {
                    Print((k == 0 ? (char)('a' + j) : ' ') + " ");

                    for (int i = 0; i < SIZE; i++) {
                        Print("|");
                        Print(lines[i, j][k], TILE_BG, colors[i, j]);
                    }

                    if (pieceToPlace != EMPTY && j == SIZE - 2) {
                        Print("|");
                        Println(new String(' ', 12) + lineToPlace[k], TILE_BG, colorToPlace);
                    } else
                        Println("|");
                }
                Println("  " + new String('-', SIZE * (TILE_SIZE + 1) + 1));
            }
        }

        static void DisplayPiecesLeft(int highlighted=-1)  {
            // Affiche toute les pièces disponibles.
            String[][] lines = new String[piecesLefts.Length][];
            ConsoleColor[] colors = new ConsoleColor[piecesLefts.Length];

            for (int p = 0; p < piecesLefts.Length; p++) {
                lines[p] = DetailPiece(piecesLefts[p]);
                colors[p] = (piecesLefts[p] & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;
            }

            if (-1 < highlighted)
                colors[highlighted] = COLOR_SEL;

            for (int k = 0; k < TILE_SIZE; k++) {
                for (int p = 0; p < piecesLefts.Length; p++) {
                    if (0 < p)
                        Print("  ");
                    Print(lines[p][k], TILE_BG, colors[p]);
                }
                Println();
            }
        }

        static void DisplayGame(int pieceToPlace=EMPTY, int selectedPiece=-1, params int[] selectedTile) {
            Console.Clear();

            Print("Player: " + currentPlayer + "\n");
            DisplayGrid(pieceToPlace, selectedTile);

            Println("Pieces lefts:");
            DisplayPiecesLeft(selectedPiece);
        }

        static int[] ChooseTile() {
            // L'entrée doit être de la forme 'a1' pour désigner une case.
            Print("Asking for XY: ");
            String input = "";

            while ((input = Console.ReadLine()).Length < 2 // Si la chaine de caractères est trop courtes,
                || input[0] < 'a' || 'a' + SIZE < input[0] // ou l'indicateur de ligne est out of range,
                || input[1] < '1' || '1' + SIZE < input[1] // ou l'indicateur de colonne est out of range,
                || grid[input[0] - 'a', input[1] - '1'] != EMPTY) // ou la case est utilisée,
                Print("Not a valide tile; asking for XY:"); // refait la saisie.

            return new int[] { input[0] - 'a', input[1] - '1' };
        }

        static int ChoosePiece(params int[] lastPlacePosXY) {
            ConsoleKey input = ConsoleKey.Clear;
            int cursor = 0;
            
            DisplayGame(EMPTY, cursor, lastPlacePosXY);

            while ((input = Console.ReadKey(true).Key) != ConsoleKey.Enter) {

                if (input == ConsoleKey.RightArrow && cursor < piecesLefts.Length - 1)
                    DisplayGame(EMPTY, ++cursor, lastPlacePosXY);

                else if (input == ConsoleKey.LeftArrow && 0 < cursor)
                    DisplayGame(EMPTY, --cursor, lastPlacePosXY);
            }
            
            return cursor;
        }

        static void Place(int pieceIndex, int[] posXY) {
            // Place la pièce.
            grid[posXY[0], posXY[1]] = piecesLefts[pieceIndex];

            // La retire de la pioche.
            int[] tmp = new int[piecesLefts.Length - 1];
            for (int k = 0; k < tmp.Length; k++)
                tmp[k] = piecesLefts[k < pieceIndex ? k : k + 1];

            piecesLefts = tmp;
        }

        static bool IsFinished(int[] lastPlacePosXY, int turnCounter, bool testForSquares=false) {
            // Initialise tout les masques à `00..011..1` (avec `2 * SIZE` '1').
            int flagsLine = ~(-1 << (2 * SIZE));
            int flagsColumn = ~(-1 << (2 * SIZE));
            int flagsDiagonal = ~(-1 << (2 * SIZE));
            int flagsAntidiagonal = ~(-1 << (2 * SIZE));

            for (int k = 0; k < SIZE && flagsLine + flagsColumn + flagsDiagonal + flagsAntidiagonal != 0; k++) {
                // Test cumulatif sur toute la ligne.
                if (flagsLine != 0 && grid[lastPlacePosXY[0], k] != EMPTY)
                    flagsLine&= ~grid[lastPlacePosXY[0], k] << SIZE | grid[lastPlacePosXY[0], k];
                else flagsLine = 0;
                // Test cumulatif sur toute la colonne.
                if (flagsColumn != 0 && grid[k, lastPlacePosXY[0]] != EMPTY)
                    flagsColumn &= ~grid[k, lastPlacePosXY[1]] << SIZE | grid[k, lastPlacePosXY[1]];
                else flagsColumn = 0;
                // Test cumulatif sur toute la diagonnale (si la derniere pièce placée est sur la diagonnale).
                if (flagsDiagonal != 0 && lastPlacePosXY[0] == lastPlacePosXY[1] && grid[k, k] != EMPTY)
                    flagsDiagonal &= ~grid[k, k] << SIZE | grid[k, k];
                else flagsDiagonal = 0;
                // Test cumulatif sur toute l'antidiagonnale (si la derniere pièce placée est sur l'antidiagonnale).
                if (flagsAntidiagonal != 0 && lastPlacePosXY[0] + lastPlacePosXY[1] == SIZE - 1 && grid[k, SIZE-1 - k] != EMPTY)
                    flagsAntidiagonal &= ~grid[k, SIZE-1 - k] << SIZE | grid[k, SIZE-1 - k];
                else flagsAntidiagonal = 0;

                /*Console.WriteLine(DetailPiece(flagsLine, SIZE) + " " + DetailPiece(flagsLine >> SIZE, SIZE));
                Console.WriteLine(DetailPiece(flagsColumn, SIZE) + " " + DetailPiece(flagsColumn >> SIZE, SIZE));
                Console.WriteLine(DetailPiece(flagsDiagonal, SIZE) + " " + DetailPiece(flagsDiagonal >> SIZE, SIZE));
                Console.WriteLine(DetailPiece(flagsAntidiagonal, SIZE) + " " + DetailPiece(flagsAntidiagonal >> SIZE, SIZE));
                Console.WriteLine();*/
            }

            // Si on a trouver (au moins) une carractéristique commune sur la ligne,
            // la colonne ou une diagonnale, la valeur correspondante sera non nulle.
            return flagsLine + flagsColumn + flagsDiagonal + flagsAntidiagonal != 0;
        }

        static String PlayGame(String player1, String player2="Computer", bool hardComputer=false) {
            // Si un des joueurs s'appel "Computer", il est remplacer par un ordinateur.
            String[] players = new String[] { player1, player2 };
            int player = 0; // Le premier joueur est le joueur `player1` (indice 0).

            int storedPieceIndex = -1;

            // Variables permettant de déterminier si la partie est finie.
            int turnCounter = 0;
            bool victory = false;

            do {
                // Affiche le plateau.
                currentPlayer = players[player];

                // Au premier tours, le joueur 1 donne une pièce au joueur 2.
                if (-1 < storedPieceIndex) {
                    DisplayGame(piecesLefts[storedPieceIndex]);

                    int[] placePosXY = ChooseTile(); // Le joueur désigne une case, puis
                    Place(storedPieceIndex, placePosXY); // place la pièce sur le plateau (la retire de la pile).
                    victory = IsFinished(placePosXY, turnCounter); // Test si c'est un coups gagnant.
                }

                DisplayGame();

                // Si la partie n'est pas finie
                if (!victory && turnCounter < 17) {
                    storedPieceIndex = ChoosePiece(); // le joueur choisis une pièce.
                    player = player == 0 ? 1 : 0; // Au suivant!
                }

                // Tant que la partie n'est pas finie.
            } while (!victory && turnCounter < 17);

            // Retourne le nom du joueur gagnant s'il y en a un, "Personne" sinon.
            return victory ? players[player] : "Nobody";
        }

    }

}

