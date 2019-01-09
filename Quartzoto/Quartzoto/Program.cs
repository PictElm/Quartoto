﻿using System;
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
        const ConsoleColor COLOR_SEL_1 = ConsoleColor.Cyan;
        const ConsoleColor COLOR_SEL_2 = ConsoleColor.Yellow;

        // Drapeaux utilisées pour une partie normale (4x4, fonctionne en deçà mais pas en delà).
        const int FLAG_IS_COL1 = 1;
        const int FLAG_IS_SQRE = 2;
        const int FLAG_IS_TALL = 4;
        const int FLAG_IS_HOLE = 8;

        // Séparateur utilisées pour les contours du plateaux (box drawing characters).
        const char SEP_HZ = '─';
        const char SEP_VE = '│';
        const char SEP_CX = '┼';

        // Taux (en poucents) d'erreur de l'ordinateur.
        static int threshold = 100;

        // Stockage du plateau.
        static int[,] grid;
        // Stockage des pieces restante : la pioche.
        static int[] piecesLefts;

        // Stockage du joueur actuel.
        static String currentPlayer;

        // Utilisé pour générer de l'aléatoire.
        static Random rng;

        /// <summary>
        /// <li>Effectuer une partie</li>
        /// <li>Adapter la difficultée</li>
        /// <li>Demander si faire une nouvelle partie</li>
        /// Le joueur commencant la première partie est selectionner aléatoirement,
        /// pour chaque autre parties, le perdant commence.
        /// </summary>
        /// <param name="args">Arguments de la ligne de commande (non utilisés).</param>
        static void Main(string[] args) {
            String name1 = "Tu", name2 = "Ordinateur";

            Print("Y a-t-il 2 joueurs ? (o/n) ");
            if (Console.ReadKey().Key == ConsoleKey.O) {
                Print("\nNom du joueur 1 : ");
                name1 = Console.ReadLine();
                Print("Nom du joueur 2 : ");
                name2 = Console.ReadLine();
            }

            // Choix aléatoire du premier joueur à commencer.
            rng = new Random();
            if (0 < rng.Next(2)) {
                String tmp = name1;
                name1 = name2;
                name2 = tmp;
            }

            do {
                // Initialise le jeux : le plateau et la pioche (et l'aléatoire).
                Initialize();

                int turnCounter = 0;

                // Fait une partie.
                String winner = PlayGame(out turnCounter, name1, name2);

                threshold+= 100 / turnCounter * (winner != "Ordinateur" ? -1 : 1);
                if (threshold < 0)
                    threshold = 0;
                if (100 < threshold)
                    threshold = 100;

                // Indique le vainqueur.
                Print("\n" + winner + " gagne!", MAIN_BG, ConsoleColor.Yellow);
                Println("       Nouvelle difficultée: " + (100 - threshold) + "%.");
                Console.Beep(440, 250);

                // Au prochain tour, le perdant commance.
                name1 = winner == name1 ? name2 : name1;
                name2 = winner;

                Print("Refaire une partie ? (o/n) ");
            } while (Console.ReadKey().Key != ConsoleKey.N);
            Println();

            // Reinitialise les couleurs de la console.
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Initialise la pioche avec toute les pièces disponible de base, rempli le plateau de `EMPTY` (pas de pièce).
        /// Crée également une nouvelle instance de rng.
        /// </summary>
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

        /// <summary>
        /// Affiche une représentation de l'objet fournit, selon les couleurs précisées.
        /// </summary>
        /// <param name="c">Objet a afficher.</param>
        /// <param name="bg">Couleur de fond.</param>
        /// <param name="fg">Couleur de police.</param>
        /// <remarks>Si la couleur de fond est la même que celle qui est deja chargée, passe l'opération (de même pour la couleur de police).</remarks>
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

        /// <summary>
        /// Fonctionne de même que `Print` mais ajoute un retour à la ligne après l'objet.
        /// </summary>
        /// <param name="c">Objet a afficher.</param>
        /// <param name="bg">Couleur de fond.</param>
        /// <param name="fg">Couleur de police.</param>
        static void Println(Object c=null, ConsoleColor bg=MAIN_BG, ConsoleColor fg=MAIN_FG) {
            // Si l'objet en paramètre est null, saute juste une ligne.
            Print(c == null ? "\n" : c.ToString() + "\n", bg, fg);
        }

        /// <summary>
        /// Détaille la pièce passée en paramètre par une série de chaine de caractères.
        /// </summary>
        /// <param name="piece">Pièce à détailler.</param>
        /// <returns>Série de chaine de caractères décrivant la pièce, ligne par ligne.</returns>
        /// <remarks>La couleur d'affichage n'est pas définie ici.</remarks>
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
            String gap = hole ? (tall ? " U_" : "  U") : (tall ? "_ _" : " __");

            // Forme exterieur de la pièce.
            String shapeR = sqre ? "[" : "(";
            String shapeL = sqre ? "]" : ")";

            // Compile le tout en ce qui s'affiche à l'écran.
            return new String[] { " " + gap[0] + " ",
                                  tall ? "" + shapeR + gap[1] + shapeL : " " + gap[1] + " ",
                                  "" + shapeR + gap[2] + shapeL };
        }

        /// <summary>
        /// Affiche le plateau à l'écran.
        /// Si la pièce à placer est précisée, elle est dessinée en parallèle (à coté du plateau).
        /// Si les coordonées de la pièce en surbriance est précisée et contient des coordonées, celle-ci est affiche en surbriance.
        /// </summary>
        /// <param name="pieceToPlace">Pièce à placer.</param>
        /// <param name="highlighted">Coordonées (liste { x, y }) de la pièce en surbriance.</param>
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
                        colors[i, j] = (pieceToPlace & FLAG_IS_COL1) != 0 ? COLOR_SEL_1 : COLOR_SEL_2;
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
                    if (pieceToPlace != EMPTY && pieceToPlace != EMPTY << SIZE && j == 1) {
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
                if (pieceToPlace != EMPTY && pieceToPlace != EMPTY << SIZE && j == 0)
                    Print(new String(' ', 12) + "Pièce a placer :");

                // Text indiquant de choisire une pièce.
                if (pieceToPlace == EMPTY && j == 0)
                    Print(new String(' ', 12) + "Choisisser une pièce.");

                Println();
            }

            Println();
            // Affiche les indices de pied de colonne.
            for (int k = 1; k < SIZE + 1; Print(new String(' ', TILE_SIZE) + k++))
                ;

            Println();
        }

        /// <summary>
        /// Affiche la pioche à l'écran.
        /// Si la pièce à placer est précisée, elle n'est pas affichée (puisqu'elle est dans la main du joueur).
        /// Si l'indice de la pièce en surbriance est précisée, celle-ci est affichée en surbriance.
        /// </summary>
        /// <param name="pieceToPlace">Pièce à placer (elle est donc déjà dessinée dans `DisplayGrid`).</param>
        /// <param name="highlighted">Indice de la pièce en surbriance.</param>
        static void DisplayPiecesLeft(int pieceToPlace=EMPTY, int highlighted=-1) {
            String[][] lines = new String[piecesLefts.Length][];
            ConsoleColor[] colors = new ConsoleColor[piecesLefts.Length];

            // Récupère les affichages pour chaques pieces dans la list de pièces restantes (la pioche).
            for (int p = 0; p < piecesLefts.Length; p++) {
                lines[p] = DetailPiece(piecesLefts[p]);
                colors[p] = (piecesLefts[p] & FLAG_IS_COL1) != 0 ? COLOR_1 : COLOR_2;
            }

            // Si une pièces est surlignée, change sa couleur.
            if (-1 < highlighted)
                colors[highlighted] = (piecesLefts[highlighted] & FLAG_IS_COL1) != 0 ? COLOR_SEL_1 : COLOR_SEL_2;

            // L'affichage découpe encore une fois chaque pièce de manière à afficher
            // l'intégralité d'une ligne de caractères avant de passer à la suivante.
            for (int k = 0; k < TILE_SIZE; k++) {
                for (int p = 0; p < piecesLefts.Length; p++)
                    if (piecesLefts[p] != pieceToPlace) {
                        if (0 < p)
                            Print("  ");
                        Print(lines[p][k], TILE_BG, colors[p]);
                    }
                Println();
            }
        }

        /// <summary>
        /// Affiche l'état du jeux avec les fonction `DisplayGrid` et `DisplayPiecesLeft`.
        /// </summary>
        /// <param name="pieceToPlace">(Si précisé) est une pièce.</param>
        /// <param name="selectedPiece">(Si précisé) est un indice dans la pioche.</param>
        /// <param name="selectedTile">(Si précisé) sont de coordones (liste { x, y }).</param>
        /// <remarks>Affiche également le nom du joueur dont c'est le tours.</remarks>
        static void DisplayGame(int pieceToPlace=EMPTY, int selectedPiece=-1, params int[] selectedTile) {
            // Nettoie la console.
            Console.Clear();

            // Affiche le nom du joueur actuel.
            Println("joueur : " + currentPlayer + "\n");
            // Affiche le plateau.
            DisplayGrid(pieceToPlace, selectedTile);
            // Affiche la pioche.
            DisplayPiecesLeft(pieceToPlace, selectedPiece);
        }

        /// <summary>
        /// Contient la boucle qui laisse le joueur choisie une case sur laquelle placer la pièce précisée.
        /// </summary>
        /// <param name="pieceToPlace">Pièce à placer.</param>
        /// <returns>Les coordonées de la case choisie (liste { x, y }).</returns>
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

        /// <summary>
        /// Contient la boucle qui laisse le joueur choisie la pièce qu'il donne à son adversaire.
        /// </summary>
        /// <param name="lastPlacePosXY">Cordonée de la dernière pièce placée.</param>
        /// <returns>L'indice dans la pioche de la pièce choisie.</returns>
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

        /// <summary>
        /// Place la pièce de la pioche désignée par l'indice aux coordonées indiquées.
        /// </summary>
        /// <param name="pieceIndex">Indice de la pièce a placer.</param>
        /// <param name="posXY">Coordonées (liste { x, y }) de la case ciblée.</param>
        static void Place(int pieceIndex, int[] posXY) {
            // Place la pièce sur le plateau.
            grid[posXY[0], posXY[1]] = piecesLefts[pieceIndex];

            // La retire de la pioche.
            int[] tmp = new int[piecesLefts.Length - 1];
            for (int k = 0; k < tmp.Length; k++)
                tmp[k] = piecesLefts[k < pieceIndex ? k : k + 1];
            piecesLefts = tmp;
        }

        /// <summary>
        /// Détermine si la partie est finie.
        /// Les test ne sont effectués que sur la colonne, la ligne et, si il y a lieux, sur les diagonnale de la dernière pièce placée.
        /// </summary>
        /// <param name="lastPlacePosXY">Coordones (liste { x, y }) de la dernière pièce placée.</param>
        /// <param name="testForSquares">[Foncionnalitée non implementée.]</param>
        /// <returns>`true` si la partie est finie (un Quarto a été trouvé), sinon `false`.</returns>
        /// <remarks>Foncinne par '&' cummulatifs.</remarks>
        static bool IsFinished(int[] lastPlacePosXY, bool testForSquares=false) {
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

        /// <summary>
        /// Permet d'éviter les 'Entrée à 4Hz' : sans, garder la touche `Enter` passe touts les menus très vite / instantanément.
        /// </summary>
        static void WaitEnterRelease() {
            //while (Console.ReadKey().Key == ConsoleKey.Enter)
            //    ;
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Effectue une partie entre les deux joueurs.
        /// Si un des joueur s'appel "Ordinateur" (joueur 2 par défaut), il est jouer par l'ordinateur.
        /// </summary>
        /// <param name="turnCounter">Compteur de tours, est inisialiser à 0.</param>
        /// <param name="player1">Nom du joueur 1.</param>
        /// <param name="player2">Nom du joueur 2, "Ordinateur" par défaut.</param>
        /// <returns>Le nom du joueur qui l'emporte.</returns>
        static String PlayGame(out int turnCounter, String player1, String player2 = "Ordinateur") {
            // Si un des joueurs s'appel "Ordinateur", il est remplacer par un ordinateur.
            String[] players = new String[] { player1, player2 };
            int player = 0; // Le premier joueur est le joueur `player1` (d'indice 0).

            // Pièce en transit (donnée par un joueur au suivant).
            int storedPieceIndex = -1;

            // Variables permettant de déterminer si la partie est finie.
            turnCounter = 0;
            bool victory = false;

            do {
                // Actualise le nom du joueur.
                currentPlayer = players[player];

                // Au premier tours, le joueur 1 donne une pièce au joueur 2 : il n'y a donc pas de placement de pièces.
                if (-1 < storedPieceIndex) {
                    // Affiche le plateau avec la pièce a placer.
                    if (currentPlayer != "Ordinateur")
                        DisplayGame(piecesLefts[storedPieceIndex]);

                    int[] placePosXY;

                    if (currentPlayer == "Ordinateur")
                        placePosXY = ComputerChooseTile(piecesLefts[storedPieceIndex], threshold); // L'ordinateur trouve une case.
                    else
                        placePosXY = ChooseTile(piecesLefts[storedPieceIndex]); // Le joueur choisis une case.

                    // Place la pièce sur le plateau (la retire de la pile).
                    Place(storedPieceIndex, placePosXY); 

                    // Test si c'est un coups gagnant.
                    victory = IsFinished(placePosXY); 
                }

                // Si la partie n'est pas finie, on passe à la selection de la pièce suivante.
                if (!victory && turnCounter < SIZE * SIZE) {
                    // Affiche le plateau et la pioche.
                    if (currentPlayer != "Ordinateur")
                        DisplayGame();

                    if (currentPlayer == "Ordinateur")
                        storedPieceIndex = ComputerChoosePiece(threshold);// L'ordinateur trouve une pièce.
                    else
                        storedPieceIndex = ChoosePiece(); // le joueur choisis une pièce.
                }
                // Si la pertie est fini, il n'y a pas lieux de choisir de nouvelle pièce.
                else {
                    // Affiche le plateau final.
                    Console.Clear();
                    Println("\n");
                    DisplayGrid(EMPTY << SIZE);
                }

                // Fin du tour pour ce joueur, au suivant!
                player = player == 0 ? 1 : 0;
            // Tant que la partie n'est pas finie.
            } while (!victory && turnCounter++ < SIZE * SIZE);

            // Fin de la partie : retourne le nom du joueur gagnant s'il y en a un, "Personne" sinon.
            return victory ? currentPlayer : "Arbitre";
        }


        ////// La suite du Program concerne l’ordinateur dans une partie //////


        /// <summary>
        /// Compte le nombre de pièce ayant la caractériqstique précisée à la valeur précisiée sur la rangée précisée.
        /// </summary>
        /// <param name="i">Si direction est "Column" : la rangée est cette colonne.</param>
        /// <param name="j">Si direction est "Line" : la rangée est cette ligne.</param>
        /// <param name="direction">"Line", "Column", "Diagonal" ou "Antidiagonal".</param>
        /// <param name="featureOffset">Carractéristique a comptée, voir les drapeaux (`FLAG_IS_...`).</param>
        /// <param name="featureSwitch">Si `false` compte les `1`, si `true` les `0`.</param>
        /// <returns>Le nombre de pièces remplissant les conditions.</returns>
        static int CountFeature(int i, int j, String direction, int featureOffset, bool featureSwitch) {
            int r = 0;

            if (direction == "Line") {
                for (int k = 0; k < SIZE; k++)
                    if (grid[i + k, j] != EMPTY && ((featureSwitch ? ~grid[i + k, j] : grid[i + k, j]) & 1 << featureOffset) != 0)
                        r++;
            }

            else if (direction == "Column") {
                for (int k = 0; k < SIZE; k++)
                    if (grid[i, j + k] != EMPTY && ((featureSwitch ? ~grid[i, j + k] : grid[i, j + k]) & 1 << featureOffset) != 0)
                        r++;
            }

            else if (direction == "Diagonal") {
                for (int k = 0; k < SIZE; k++)
                    if (grid[k, k] != EMPTY && ((featureSwitch ? ~grid[k, k] : grid[k, k]) & 1 << featureOffset) != 0)
                        r++;
            }

            else if (direction == "Antidiagonal") {
                for (int k = 0; k < SIZE; k++)
                    if (grid[k, SIZE-1 - k] != EMPTY && ((featureSwitch ? ~grid[k, SIZE-1 - k] : grid[k, SIZE-1 - k]) & 1 << featureOffset) != 0)
                        r++;
            }

            return r;
        }

        /// <summary>
        /// Choix d'une case par l'ordinateur.
        /// </summary>
        /// <param name="pieceToPlace">Pièce à placer.</param>
        /// <param name="randomThreshold">Seuil (en %) d'aléatoire (équivalent à un taux d'erreur).</param>
        /// <returns></returns>
        static int[] ComputerChooseTile(int pieceToPlace, int randomThreshold=100) {
            // Ordinateur en mode aléatoire.
            if (rng.Next(100) < randomThreshold) {
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
                    if (CountFeature(0, i, "Line", k, (pieceToPlace & 1 << k) == 0) == SIZE - 1)
                        for (int j = 0; j < SIZE; j++) // Si 3 pièces partage une caractéristique, cherche une case vide.
                            if (grid[j, i] == EMPTY)
                                return new int[] { j, i };

                    // Pour chaque colonne.
                    if (CountFeature(i, 0, "Column", k, (pieceToPlace & 1 << k) == 0) == SIZE - 1)
                        for (int j = 0; j < SIZE; j++)
                            if (grid[i, j] == EMPTY)
                                return new int[] { i, j };
                }

                // Pour la diagonnale.
                if (CountFeature(0, 0, "Diagonal", k, (pieceToPlace & 1 << k) == 0) == SIZE - 1)
                    for (int i = 0; i < SIZE; i++)
                        if (grid[i, i] == EMPTY)
                            return new int[] { i, i };

                // Pour la l'antidiagonnale.
                if (CountFeature(0, 0, "Antidiagonal", k, (pieceToPlace & 1 << k) == 0) == SIZE - 1)
                    for (int j = 0; j < SIZE; j++)
                        if (grid[j, SIZE-1 - j] == EMPTY)
                            return new int[] { j, SIZE-1 - j };
            }

            // S'il n'y a pas de début d'alignement sur 3 cases (ou que la 4-ème cases était prises).
            return ComputerChooseTile(pieceToPlace);
        }

        /// <summary>
        /// Choix d'une pièce par l'ordinateur.
        /// </summary>
        /// <param name="randomThreshold">Seuil (en %) d'aléatoire (équivalent à un taux d'erreur).</param>
        /// <returns>L'indice dans la pioche de la pièce choisie.</returns>
        static int ComputerChoosePiece(int randomThreshold=100) {
            // Ordinateur en mode aléatoire.
            if (rng.Next(100) < randomThreshold)
                // Retourne l'indice d'une case au hasard.
                return rng.Next(piecesLefts.Length);

            //int[] shuffled = piecesLefts.OrderBy(e => rng.Next()).ToArray();

            for (int p = 0; p < piecesLefts.Length; p++) {
                bool canWinWithP = false;

                for (int k = 0; k < SIZE; k++) {
                    for (int i = 0; i < SIZE; i++) {

                        // Pour chaque lignes, compte les 1 en `k`-ème bit, puis les 0.
                        if (CountFeature(0, i, "Line", k, (piecesLefts[p] & 1 << k) == 0) == SIZE - 1)
                            for (int j = 0; j < SIZE; j++) // Si 3 pièces partage une caractéristique, cherche une case vide.
                                if (grid[j, i] == EMPTY) // S'il y a une case vide, c'est que le joueur peut gagnier.
                                    canWinWithP = true;

                        // Pour chaque colonne.
                        if (CountFeature(i, 0, "Column", k, (piecesLefts[p] & 1 << k) == 0) == SIZE - 1)
                            for (int j = 0; j < SIZE; j++)
                                if (grid[i, j] == EMPTY)
                                    canWinWithP = true;
                    }

                    // Pour la diagonnale.
                    if (CountFeature(0, 0, "Diagonal", k, (piecesLefts[p] & 1 << k) == 0) == SIZE - 1)
                        for (int i = 0; i < SIZE; i++)
                            if (grid[i, i] == EMPTY)
                                canWinWithP = true;

                    // Pour la l'antidiagonnale.
                    if (CountFeature(0, 0, "Antidiagonal", k, (piecesLefts[p] & 1 << k) == 0) == SIZE - 1)
                        for (int j = 0; j < SIZE; j++)
                            if (grid[j, SIZE-1 - j] == EMPTY)
                                canWinWithP = true;
                }

                if (!canWinWithP)
                    return p;
            }

            return ComputerChoosePiece();
        }

    }

}

