using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartzoto {

    class Program {

        const int SIZE = 4;
        const int TILE_SIZE = 1;

        static int[,] grid;
        static int[] piecesLefts;

        static String currentPlayer;

        static void Main(string[] args) {
            Initialize(); // Initialise.
            String winner = PlayGame("Alexa", "Patrick"); // Fait une partie entre "Alexa" et "Patrick".
            Console.WriteLine(winner + " wins!");
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
                    grid[i, j] = -1;
        }

        static String[] DetailPiece(int piece) {
            return new String[] { "µ" };
        }

        static void DisplayGrid(params int[] higlighted) {
            Console.Clear();

            String[,][] lines = new String[SIZE, SIZE][];
            ConsoleColor[,] colors = new ConsoleColor[SIZE, SIZE];

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++) {
                    lines[i, j] = DetailPiece(grid[i, j]);
                    colors[i, j] = grid[i, j] == -1 ? ConsoleColor.Gray : (grid[i, j] & 1) == 0 ? ConsoleColor.White : ConsoleColor.Black;
                }

            Console.WriteLine("Player: " + currentPlayer + "\n");
            Console.WriteLine("  " + new String('-', SIZE * (TILE_SIZE + 1)));

            for (int j = 0; j < SIZE; j++) {
                for (int k = 0; k < TILE_SIZE; k++) {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(k == 0 ? (char)('a' + j) : ' ');
                    Console.Write(" ");
                    Console.BackgroundColor = ConsoleColor.Black;

                    for (int i = 0; i < SIZE; i++) {
                        Console.ForegroundColor = colors[i, j];
                        Console.Write("|" + lines[i, j][k]);
                    }
                    Console.WriteLine("|");
                }
                Console.WriteLine("  " + new String('-', SIZE * (TILE_SIZE + 1)));
            }
        }

        static int[] ChooseTile() {
            // L'entrée doit être de la forme 'a1' pour désigner une case.
            Console.Write("Asking for XY: ");
            String input = "";

            while ((input = Console.ReadLine()).Length < 2 // Si la chaine de caractères est trop courtes,
                || input[0] < 'a' || 'a' + SIZE < input[0] // ou l'indicateur de ligne est out of range,
                || input[1] < '1' || '1' + SIZE < input[1] // ou l'indicateur de colonne est out of range,
                || grid[input[0] - 'a', input[1] - '1'] != -1) // ou la case est utilisée,
                Console.Write("Not a valide tile; asking for XY:"); // refait la saisie.

            return new int[] { input[0] - 'a', input[1] - '1' };
        }

        static int ChoosePiece() {
            // Affiche toute les pièces disponibles.
            String tmp = "\n";
            for (int k = 0; k < piecesLefts.Length; tmp+= "\t" + (k + 1) + ":\t" + DetailPiece(piecesLefts[k++]) + "\n")
                ;
            Console.WriteLine("Pieces lefts: " + tmp);

            Console.WriteLine("Asking for index in {0}, {1}: ", 1, piecesLefts.Length);
            int input = -1;

            // Tant que la sasie n'est pas valide (hors de l'intervale [1, `piecesLefts.Length`]), refait la saisie.
            while ((input = int.Parse(Console.ReadLine())) < 0 || input > piecesLefts.Length)
                Console.Write("Not a valide index; asking for index in {0}, {1}: ", 1, piecesLefts.Length);

            // Array starts at 0.
            return input - 1;
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
                if (flagsLine != 0 && grid[lastPlacePosXY[0], k] != -1)
                    flagsLine&= ~grid[lastPlacePosXY[0], k] << SIZE | grid[lastPlacePosXY[0], k];
                else flagsLine = 0;
                // Test cumulatif sur toute la colonne.
                if (flagsColumn != 0 && grid[k, lastPlacePosXY[0]] != -1)
                    flagsColumn &= ~grid[k, lastPlacePosXY[1]] << SIZE | grid[k, lastPlacePosXY[1]];
                else flagsColumn = 0;
                // Test cumulatif sur toute la diagonnale (si la derniere pièce placée est sur la diagonnale).
                if (flagsDiagonal != 0 && lastPlacePosXY[0] == lastPlacePosXY[1] && grid[k, k] != -1)
                    flagsDiagonal &= ~grid[k, k] << SIZE | grid[k, k];
                else flagsDiagonal = 0;
                // Test cumulatif sur toute l'antidiagonnale (si la derniere pièce placée est sur l'antidiagonnale).
                if (flagsAntidiagonal != 0 && lastPlacePosXY[0] + lastPlacePosXY[1] == SIZE - 1 && grid[k, SIZE-1 - k] != -1)
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
                DisplayGrid();

                // Au premier tours, le joueur 1 donne une pièce au joueur 2.
                if (-1 < storedPieceIndex) {
                    int[] placePosXY = ChooseTile(); // Le joueur désigne une case, puis
                    Place(storedPieceIndex, placePosXY); // place la pièce sur le plateau (la retire de la pile).
                    victory = IsFinished(placePosXY, turnCounter); // Test si c'est un coups gagnant.
                    
                    // Réactualise l'affichage.
                    DisplayGrid();
                }

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

