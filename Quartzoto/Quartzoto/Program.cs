using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartzoto {

    class Program {

        // Le jeux s'adapte à d'autre taille (notament 2x2, c'est plus facile de gagner :3)
        const int SIZE = 4;
        // Taille d'une pièce et donc également de la case qu'elle occupe.
        const int TILE_SIZE = 3;
        // Désigne une case vide (parce que 0 désigne la pièce rouge, petite, ronde et plate).
        const int EMPTY = -1;

        // Couleurs de fond principale et des cases du plateau.
        const ConsoleColor MAIN_BG = ConsoleColor.Black;
        const ConsoleColor TILE_BG = ConsoleColor.Black;

        // Couleur de text principales utilisées pour l'affichage des contours du plateau par exemple.
        const ConsoleColor MAIN_FG = ConsoleColor.Gray;
        // Couleurs utilisées par les pièces.
        const ConsoleColor COLOR_1 = ConsoleColor.Blue;
        const ConsoleColor COLOR_2 = ConsoleColor.Red;
        // Couleur de la pièce sélectionnée.
        const ConsoleColor COLOR_SEL = ConsoleColor.Green;

        // Drapeaux utilisées pour une partie normale (4x4, fonctionne en deçà mais pas en delà).
        const int FLAG_IS_COL1 = 1;
        const int FLAG_IS_SQRE = 2;
        const int FLAG_IS_TALL = 4;
        const int FLAG_IS_HOLE = 8;

        // Séparateur utilisées pour les contours du plateaux (box drawing characters).
        const char SEP_HZ = '─';
        const char SEP_VE = '│';
        const char SEP_CX = '┼';

        // Stockage du plateau.
        static int[,] grid;
        // Stockage des pieces restante : la pioche.
        static int[] piecesLefts;

        // Stockage du joueur actuel.
        static String currentPlayer;

        // Utilisé pour générer de l'aléatoire.
        static Random rng;

        static void Main(string[] args) {
            // Initialise le jeux : le plateau et la pioche (et l'aléatoire).
            Initialize();

            // Fait une partie contre l'ordinateur.
            String winner = PlayGame("Moi");

            // Indique le vainqueur.
            Println("\n" + winner + " wins!\n", MAIN_BG, ConsoleColor.Yellow);

            // Reinitialise les couleurs de la console.
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
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

            // Initialise l'aléatoire.
            rng = new Random();
        }

        static void Print(Object c, ConsoleColor bg=MAIN_BG, ConsoleColor fg=MAIN_FG) {
            // Change la couleur uniquement si nessecaire.
            if (Console.BackgroundColor != bg)
                Console.BackgroundColor = bg;

            // Change la couleur uniquement si nessecaire.
            if (Console.ForegroundColor != fg)
                Console.ForegroundColor = fg;
            
            // Affiche le message.
            Console.Write(c);
        }

        static void Println(Object c=null, ConsoleColor bg=MAIN_BG, ConsoleColor fg=MAIN_FG) {
            // Si l'objet en paramètre est null, saute juste une ligne.
            Print(c == null ? "\n" : c.ToString() + "\n", bg, fg);
        }

        static String[] DetailPiece(int piece) {
            // Si la piece en paramètre est une case vide, retourne des chaines de caractères vides.
            if (piece == EMPTY) {
                String[] r = new String[TILE_SIZE];
                for (int k = 0; k < TILE_SIZE; r[k++] = new String(' ', TILE_SIZE))
                    ;
                return r;
            }

            bool sqre = (piece & FLAG_IS_SQRE) != 0; // Es-ce que la pièce est carrée ?
            bool tall = (piece & FLAG_IS_TALL) != 0; // Es-ce que la pièce est grande ?
            bool hole = (piece & FLAG_IS_HOLE) != 0; // Es-ce que la pièce est percée ?

            // Détermine comment devrais se comporté la colonne centrale (varie selon la taille et le perçage).
            String gap = hole ? (tall ? " V_" : "  V") : (tall ? "_ _" : " __");

            // Forme exterieur de la pièce.
            String shapeR = sqre ? "[" : "(";
            String shapeL = sqre ? "]" : ")";

            // Compile le tout en ce qui s'affiche à l'écran.
            return new String[] { " " + gap[0] + " ",
                                  tall ? "" + shapeR + gap[1] + shapeL : " " + gap[1] + " ",
                                  "" + shapeR + gap[2] + shapeL };
        }

        static void DisplayGrid(int pieceToPlace=EMPTY, params int[] highlighted) {
            String[,][] lines = new String[SIZE, SIZE][];
            ConsoleColor[,] colors = new ConsoleColor[SIZE, SIZE];

            String[] lineToPlace = DetailPiece(pieceToPlace);
            ConsoleColor colorToPlace = (pieceToPlace & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;

            // On parcour le plateau pour récupérer les affichage distinct des pieces présente (couleur et chaines de caractères).
            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++) {
                    // S'il y a une pièce en selection, elle est 'placée' par dessus le plateau.
                    if (highlighted.Length == 2 && highlighted[0] == i && highlighted[1] == j) {
                        lines[i, j] = lineToPlace;
                        colors[i, j] = COLOR_SEL;
                    } else {
                        lines[i, j] = DetailPiece(grid[i, j]);
                        colors[i, j] = grid[i, j] == EMPTY ? MAIN_FG : (grid[i, j] & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;
                    }
                }

            // Affiche la premère ligne de séparation horizontale.
            // Cette ligne est ponctuée de '+' séparés par `TILE_SIZE` espaces.
            // '┼────┼────┼───..──┼'
            Print("  " + SEP_CX);
            for (int i = 0; i < SIZE; i++)
                Print(new String(SEP_HZ, TILE_SIZE) + SEP_CX);

            Println();

            // L'affichage découpe chaque pièce de manière à afficher l'intégralité
            // de la première rangée avant de passer à la suivante.
            // Ainsi `j` parcoure le plateau à la verticale et `i` à l'horizontale.
            for (int j = 0; j < SIZE; j++) {
                // Affichage d'une ligne de caractères.
                for (int k = 0; k < TILE_SIZE; k++) {
                    // Ajoute les indice de début de ligne.
                    Print((k == 0 ? (char)('a' + j) : ' ') + " ");

                    for (int i = 0; i < SIZE; i++) {
                        Print(SEP_VE); // Affiche les séparateurs verticaux dans la couleur principale.
                        Print(lines[i, j][k], TILE_BG, colors[i, j]);
                    }

                    // S'il y a une pièce à placer elle est ajoutée 1 ligne par 1 ligne à côté du tableau.
                    // Affiche également les séparateurs verticaux les plus à droite.
                    if (pieceToPlace != EMPTY && j == 1) {
                        Print(SEP_VE);
                        Println(new String(' ', 16) + lineToPlace[k], TILE_BG, colorToPlace);
                    } else
                        Println(SEP_VE);
                }

                // Affichage de la dernière ligne de séparation horizontale (la plus en bas).
                Print("  " + SEP_CX);
                for (int i = 0; i < SIZE; i++)
                    Print(new String(SEP_HZ, TILE_SIZE) + SEP_CX);

                // Text au dessus de la pièce à placer.
                if (pieceToPlace != EMPTY && j == 0)
                    Print(new String(' ', 12) + "Piece to place:");

                Println();
            }

            Println();
            // Affiche les indices de pied de colonne.
            for (int k = 1; k < SIZE + 1; Print(new String(' ', TILE_SIZE) + k++))
                ;

            Println();
        }

        static void DisplayPiecesLeft(int highlighted=-1)  {
            String[][] lines = new String[piecesLefts.Length][];
            ConsoleColor[] colors = new ConsoleColor[piecesLefts.Length];

            // Récupère les affichages pour chaques pieces dans la list de pièces restantes (la pioche).
            for (int p = 0; p < piecesLefts.Length; p++) {
                lines[p] = DetailPiece(piecesLefts[p]);
                colors[p] = (piecesLefts[p] & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;
            }

            // Si une pièces est surlignée, change sa couleur.
            if (-1 < highlighted)
                colors[highlighted] = COLOR_SEL;

            // L'affichage découpe encore une fois chaque pièce de manière à afficher
            // l'intégralité d'une ligne de caractères avant de passer à la suivante.
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
            // Nettoie la console.
            Console.Clear();

            // Affiche le nom du joueur actuel.
            Println("Player: " + currentPlayer + "\n");
            // Affiche le plateau.
            DisplayGrid(pieceToPlace, selectedTile);
            // Affiche la pioche.
            DisplayPiecesLeft(selectedPiece);
        }

        static int[] ChooseTile(int pieceToPlace) {
            ConsoleKey input = ConsoleKey.Clear;
            int x = SIZE / 2, y = SIZE / 2;

            DisplayGame(pieceToPlace, -1, x, y);

            // Déplace le curseur selon les entrées clavier de l'utilisateur récupérées avec `ReadKey`.
            // La boucle se termine lorsque l'utilisateur appui sur 'entrer'.
            while ((input = Console.ReadKey(true).Key) != ConsoleKey.Enter
                    || grid[x, y] != EMPTY) {

                // Ces test font en sorte que le curseur ne sorte pas du plateau.
                if (input == ConsoleKey.RightArrow && x < SIZE - 1)
                    DisplayGame(pieceToPlace, -1, ++x, y); // Réactualisation de l'affichage.

                else if (input == ConsoleKey.LeftArrow && 0 < x)
                    DisplayGame(pieceToPlace, -1, --x, y);

                else if (input == ConsoleKey.DownArrow && y < SIZE - 1)
                    DisplayGame(pieceToPlace, -1, x, ++y);
                
                else if (input == ConsoleKey.UpArrow && 0 < y)
                    DisplayGame(pieceToPlace, -1, x, --y);
            }

            return new int[] { x, y };
        }

        static int ChoosePiece(params int[] lastPlacePosXY) {
            ConsoleKey input = ConsoleKey.Clear;
            int cursor = piecesLefts.Length / 2;
            
            DisplayGame(EMPTY, cursor, lastPlacePosXY);

            // Déplace le curseur selon les entrées clavier de l'utilisateur récupérées avec `ReadKey`.
            // La boucle se termine lorsque l'utilisateur appui sur 'entrer'.
            while ((input = Console.ReadKey(true).Key) != ConsoleKey.Enter) {

                // Ces test font en sorte que le curseur ne sorte pas de la pioche.
                if (input == ConsoleKey.RightArrow && cursor < piecesLefts.Length - 1)
                    DisplayGame(EMPTY, ++cursor, lastPlacePosXY); // Réactualisation de l'affichage.

                else if (input == ConsoleKey.LeftArrow && 0 < cursor)
                    DisplayGame(EMPTY, --cursor, lastPlacePosXY);
            }
            
            return cursor;
        }

        static void Place(int pieceIndex, int[] posXY) {
            // Place la pièce sur le plateau.
            grid[posXY[0], posXY[1]] = piecesLefts[pieceIndex];

            // La retire de la pioche.
            int[] tmp = new int[piecesLefts.Length - 1];
            for (int k = 0; k < tmp.Length; k++)
                tmp[k] = piecesLefts[k < pieceIndex ? k : k + 1];
            piecesLefts = tmp;
        }

        static bool IsFinished(int[] lastPlacePosXY, int turnCounter, bool testForSquares=false) {
            /* On cherche a déterminer s'il existe une caractériqtique redondante sur
             * la ligne, la colonne ou éventuelement une des diagonnale sur laquel on vient de jouer.
             * 
             * Pour ce faire, on réalise un test logique '&' d’agrégation sur toute la suite d'élements.
             *      suite (peut-être la ligne) :
             *            0101 ;  1101 ;  0011 ;  1001
             *      on obtient :
             *            0001
             *
             * Dans ce cas là, le résultat est non nul. On en déduit qu'il y a une caractéristique commune
             * sur la ligne : c'est la couleur (voir les définitions des drapeau).
             **/

            // Initialise tout les valeurs à `00..011..1` (avec `2 * SIZE` '1').
            int flagsLine = ~(-1 << (2 * SIZE));
            int flagsColumn = ~(-1 << (2 * SIZE));
            int flagsDiagonal = ~(-1 << (2 * SIZE));
            int flagsAntidiagonal = ~(-1 << (2 * SIZE));

            // Pour des raisons d'optimisation, la boucle se termine dès lors qu'il ne peut pas y avoir
            // de caractéristique commune ; e.g.:  0001 ;  1000 ;  .. (le reste n'import pas).
            for (int k = 0; k < SIZE && flagsLine + flagsColumn + flagsDiagonal + flagsAntidiagonal != 0; k++) {

                // Test cumulatif sur toute la ligne.
                if (flagsLine != 0 && grid[lastPlacePosXY[0], k] != EMPTY)
                    flagsLine&= ~grid[lastPlacePosXY[0], k] << SIZE | grid[lastPlacePosXY[0], k];
                else flagsLine = 0;

                // Test cumulatif sur toute la colonne.
                if (flagsColumn != 0 && grid[k, lastPlacePosXY[1]] != EMPTY)
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
            }

            // Si on a trouver (au moins) une carractéristique commune sur la ligne,
            // la colonne ou une diagonnale, la valeur correspondante sera non nulle
            // et donc la somme non plus.
            return flagsLine + flagsColumn + flagsDiagonal + flagsAntidiagonal != 0;
        }

        static String PlayGame(String player1, String player2="Computer", bool hardComputer=false) {
            // Si un des joueurs s'appel "Computer", il est remplacer par un ordinateur.
            String[] players = new String[] { player1, player2 };
            int player = 0; // Le premier joueur est le joueur `player1` (d'indice 0).

            // Pièce en transit (donnée par un joueur au suivant).
            int storedPieceIndex = -1;

            // Variables permettant de déterminer si la partie est finie.
            int turnCounter = 0;
            bool victory = false;

            do {
                // Actualise le nom du joueur.
                currentPlayer = players[player];

                // Au premier tours, le joueur 1 donne une pièce au joueur 2 : il n'y a donc pas de placement de pièces.
                if (-1 < storedPieceIndex) {
                    // Affiche le plateau avec la pièce a placer.
                    DisplayGame(piecesLefts[storedPieceIndex]);

                    int[] placePosXY;

                    if (currentPlayer == "Computer")
                        placePosXY = ComputerChooseTile(piecesLefts[storedPieceIndex]); // L'ordinateur trouve une case.
                    else
                        placePosXY = ChooseTile(piecesLefts[storedPieceIndex]); // Le joueur choisis une case.

                    // Place la pièce sur le plateau (la retire de la pile).
                    Place(storedPieceIndex, placePosXY); 

                    // Test si c'est un coups gagnant.
                    victory = IsFinished(placePosXY, turnCounter); 
                }

                // Si la partie n'est pas finie, on passe à la selection de la pièce suivante.
                if (!victory && turnCounter < SIZE * SIZE - 1) {
                    // Affiche le plateau et la pioche.
                    DisplayGame();

                    if (currentPlayer == "Computer")
                        storedPieceIndex = ComputerChoosePiece();// L'ordinateur trouve une pièce.
                    else
                        storedPieceIndex = ChoosePiece(); // le joueur choisis une pièce.
                }
                // Si la pertie est fini, il n'y a pas lieux de choisir de nouvelle pièce.
                else {
                    // Affiche le plateau final.
                    Println("Player: " + currentPlayer + "\n");
                    DisplayGrid();
                }

                // Fin du tour pour ce joueur, au suivant!
                player = player == 0 ? 1 : 0;
            // Tant que la partie n'est pas finie.
            } while (!victory && turnCounter++ < SIZE * SIZE - 1);

            // Fin de la partie : retourne le nom du joueur gagnant s'il y en a un, "Personne" sinon.
            return victory ? currentPlayer : "Nobody";
        }


        ////// La suite du Program concerne l’ordinateur dans une partie //////

        static int CountFeature(int i, int j, String direction, int featureOffset, bool featureSwitch) {
            int r = 0;

            if (direction == "Line")
                for (int k = 0; k < SIZE; r += ((featureSwitch ? ~grid[i + k++, j] : grid[i + k++, j]) & 1 << featureOffset) != 0 ? 1 : 0)
                    ;

            else if (direction == "Column")
                for (int k = 0; k < SIZE; r += ((featureSwitch ? ~grid[i, j + k++] : grid[i, j + k++]) & 1 << featureOffset) != 0 ? 1 : 0)
                    ;

            else if (direction == "Diagonal")
                for (int k = 0; k < SIZE; r += ((featureSwitch ? ~grid[k, k++] : grid[k, k++]) & 1 << featureOffset) != 0 ? 1 : 0)
                    ;

            else if (direction == "Antidiagonal")
                for (int k = 0; k < SIZE; r += ((featureSwitch ? ~grid[k, SIZE-1 - k++] : grid[k, SIZE-1 - k++]) & 1 << featureOffset) != 0 ? 1 : 0)
                    ;

            return r;
        }

        static int[] ComputerChooseTile(int pieceToPlace, bool isRandom=true) {
            // Ordinateur en mode aléatoire.
            if (isRandom) {
                int x, y;
                // Retourne la première case trouver non occupée.
                while (grid[x = rng.Next(SIZE), y = rng.Next(SIZE)] != EMPTY)
                    ;
                return new int[] { x, y };
            }

            // Essaie de trouver une ligne où 3 pièces partage une caractéristique.
            // `k` parcour toute les caractéristiques.
            for (int k = 0; k < SIZE; k++) {
                for (int i = 0; i < SIZE; i++) {

                    // Pour chaque lignes, compte les 1 en `k`-ème bit, puis les 0.
                    if (CountFeature(i, 0, "Line", k, false) == SIZE - 1 || CountFeature(i, 0, "Line", k, true) == SIZE - 1)
                        for (int j = 0; j < SIZE; j++) // Si 3 pièces partage une caractéristique, cherche une case vide.
                            if (grid[i, j] == EMPTY)
                                return new int[] { i, j };

                    // Pour chaque colonne.
                    if (CountFeature(i, 0, "Column", k, false) == SIZE - 1 || CountFeature(i, 0, "Column", k, true) == SIZE - 1)
                        for (int j = 0; j < SIZE; j++)
                            if (grid[j, i] == EMPTY)
                                return new int[] { j, i };
                }

                // Pour la diagonnale.
                if (CountFeature(0, 0, "Diagonal", k, false) == SIZE - 1 || CountFeature(0, 0, "Diagonal", k, true) == SIZE - 1)
                    for (int i = 0; i < SIZE; i++)
                        if (grid[i, i] == EMPTY)
                            return new int[] { i, i };

                // Pour la l'antidiagonnale.
                if (CountFeature(0, 0, "Antidiagonal", k, false) == SIZE - 1 || CountFeature(0, 0, "Antidiagonal", k, true) == SIZE - 1)
                    for (int j = 0; j < SIZE; j++)
                        if (grid[j, SIZE-1 - j] == EMPTY)
                            return new int[] { j, SIZE-1 - j };
            }

            // S'il n'y a pas de début d'alignement sur 3 cases (ou que la 4-ème cases était prises).
            return ComputerChooseTile(pieceToPlace);
        }

        static int ComputerChoosePiece(bool isRandom=true) {
            // Ordinateur en mode aléatoire.
            if (isRandom)
                // Retourne l'indice d'une case au hasard.
                return rng.Next(piecesLefts.Length);

            return ComputerChoosePiece();
        }

    }

}

