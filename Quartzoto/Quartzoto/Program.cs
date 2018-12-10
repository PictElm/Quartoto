using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartzoto {

    class Program {

        const int SIZE = 4;

        static int[,] grid = new int[SIZE, SIZE];
        static int[] piecesLefts = new int[16];

        static void Main(string[] args) {
            Initialize();
            String winner = PlayGame(new String[] { "Patric", "Michelle" });
            Console.WriteLine(winner + " won!");
        }

        static void Initialize() {
            for (int k = 0; k < 8; k++) {
                piecesLefts[k] = k + 1;
                piecesLefts[8 + k] = (k + 1) << 4;
            }
        }

        static void DisplayGrid(params int[] higlighted) {
            String r = "";
            int tileLength = 4;

            for (int i = 0; i < 2 * SIZE + 1; i++) {
                // Les lignes avec séparateurs n'ont pas d'indicateurs: elle commences par 2 espaces.
                if (i % 2 == 0)
                    r += "  ";
                // Sinon affiche les indicateur de lignes (a, b, c et d si TAILLE = 4).
                else
                    r += (char)('a' + i / 2) + " ";

                for (int j = 0; j < 2 * SIZE + 1; j++) {
                    // Une ligne sur deux est un séparateur (+----+----+--..).
                    if (i % 2 == 0)
                        r += j % 2 == 0 ? "+" : new String('-', tileLength);
                    else {
                        String tmp = j % 2 == 0 ? "" : grid[i / 2, j / 2].ToString();
                        // Une colonne sur deux est un séparateur (|blab|coco|fo..).
                        r += j % 2 == 0 ? "|" : (tmp + new String(' ', tileLength - tmp.Length));
                    }
                }

                // Retour à la ligne.
                r += "\n";
            }

            // Finalement affiche les indicateur de colonnes (1, 2, 3 et 4 si TAILLE = 4).
            r += "\n   ";
            for (int k = 1; k < SIZE + 1; r += k++ + new String(' ', 4))
                ;

            Console.WriteLine(r);
        }

        static bool IsFinished(int[] lastPlacePosXY, int turnCounter, bool testForSquares=false) {
            int flags = -1;

            return flags != 0;
        }

        static int[] ChooseTile() {
            Console.Write("Asking for XY: ");
            String input = "";

            while ((input = Console.ReadLine()).Length < 2 // Si la chaine de caractères est trop courtes,
                || input[0] < 'a' || 'a' + SIZE < input[0] // ou l'indicateur de ligne est out of range,
                || input[1] < '1' || '1' + SIZE < input[1]) // ou l'indicateur de colonne est out of range.
                Console.Write("Not a valide tile; asking for XY:");

            return new int[] { input[0] - 'a', input[1] - '1' };
        }

        static int ChoosePiece() {
            Console.WriteLine("Asking for index in {0}, {1}: ", 1, piecesLefts.Length);
            int input = -1;

            while ((input = int.Parse(Console.ReadLine())) < 0 || input > piecesLefts.Length)
                Console.Write("Not a valide index; asking for index in {0}, {1}: ", 1, piecesLefts.Length);

            return input - 1;
        }

        static void Place(int pieceIndex, int[] posXY) {
            grid[posXY[0], posXY[1]] = piecesLefts[pieceIndex];

            int[] tmp = new int[piecesLefts.Length - 1];
            for (int k = 0; k < tmp.Length; k++)
                tmp[k] = piecesLefts[k < pieceIndex ? k : k + 1];
            piecesLefts = tmp;
        }

        static String PlayGame(String[] players, bool secondPlayer=true, bool hardComputer=false) {
            int storedPieceIndex = -1;
            int player = 0;
            int turnCounter = 0;
            bool victory = false;

            do {
                player = player == 0 ? 1 : 0;

                Console.WriteLine("Player: " + players[player] + "\n");
                DisplayGrid();
                Console.WriteLine();

                if (-1 < storedPieceIndex) {
                    int[] placePosXY = ChooseTile();
                    Place(storedPieceIndex, placePosXY);
                    victory = IsFinished(placePosXY, turnCounter);
                }

                storedPieceIndex = ChoosePiece();
            } while (!victory && turnCounter < 17);

            return victory ? players[player] : "Nobody";
        }

    }

}

