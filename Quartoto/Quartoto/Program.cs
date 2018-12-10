using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetInfo
{

    class Program
    {
        static int TAILLE = 4;
        static String CASE_VIDE = "    ";

        static Random RNG;

        static String[] piecesRestantes;
        static String[,] grille;

        static public void Main(String[] args) {
            FaireUnePartie(true);
        }

        static public void InitialiserPartie() {
            piecesRestantes = new String[] { "nptr", "nptc", "npPr", "npPc",
                                             "bptr", "bptc", "bpPr", "bpPc",
                                             "ngtr", "ngtc", "ngPr", "ngPc",
                                             "bgtr", "bgtc", "bgPr", "bgPc" };

            grille = new String[TAILLE, TAILLE];
            for (int i = 0; i < TAILLE; i++)
                for (int j = 0; j < TAILLE; j++)
                    grille[i, j] = CASE_VIDE;

            RNG = new Random();
        }

        static void Afficher() {
            String r = "";

            for (int i = 0; i < 2 * TAILLE + 1; i++) {
                // Les lignes avec séparateurs n'ont pas d'indicateurs: elle commences par 2 espaces.
                if (i % 2 == 0)
                    r+= "  ";
                // Sinon affiche les indicateur de lignes (a, b, c et d si TAILLE = 4).
                else
                    r+= (char)('a' + i / 2) + " ";

                for (int j = 0; j < 2 * TAILLE + 1; j++) {
                    // Une ligne sur deux est un séparateur (+----+----+--..).
                    if (i % 2 == 0)
                        r+= j % 2 == 0 ? "+" : new String('-', 4);
                    else
                        // Une colonne sur deux est un séparateur (|blab|coco|fo..).
                        r+= j % 2 == 0 ? "|" : grille[i / 2, j / 2];
                }

                // Retour à la ligne.
                r+= "\n";
            }

            // Finalement affiche les indicateur de colonnes (1, 2, 3 et 4 si TAILLE = 4).
            r += "\n   ";
            for (int k = 1; k < TAILLE + 1; r+= k++ + CASE_VIDE)
                ;

            Console.WriteLine(r);
        }

        static int FaireUnePartie(bool deuxJoueurs = false) {
            InitialiserPartie();

            int joueur = 1;
            int tour = 0;
            int indicePieceSelectionnee = -1;

            Console.WriteLine("Le joueur 1 choisis la pièce pour le joueur 2 parmis: \n" + DetaillerRestantes());
            Console.Write("Choisir une pièce pour l'autre joueur: ");

            // Tant que l'entrée ne corespond pas a une pièce.
            while ((indicePieceSelectionnee = int.Parse(Console.ReadLine()) - 1) < 0
                   || piecesRestantes.Length - 1 < indicePieceSelectionnee)
                Console.Write("Veuiller entrer une pièce valable: ");

            // Tours de jeux.
            do {
                /***************************************************************
                 * Passe au joueur suivant (le j 2 place en premier) et affiche.
                 */
                joueur = joueur == 1 ? 2 : 1;
                Console.WriteLine("\n\n");
                Afficher();
                Console.WriteLine("\n");

                /**********************************************
                 * Phase 1: placer la pièce sélectionnée avant.
                 */
                Console.WriteLine("Le joueur {0} doit poser une pièce: " + DetaillerPiece(piecesRestantes[indicePieceSelectionnee]), joueur);
                Console.Write("Où jouer ? ");

                int[] choix = null;

                // Cas d'un joueur humain.
                if (deuxJoueurs || joueur == 1)
                    while ((choix = TraduireEntree(Console.ReadLine())) == null // L'entrée est de format 'a1' et doit être traduite.
                           || EstOccuper(choix[0], choix[1])) // Tant que l'entrée ne corespond pas a une case libre.
                        Console.Write("Veuiller entrer une case valable: ");

                // Cas du joueur ordinateur (fonctions Reflechir[..]).
                else {
                    choix = ReflechirPlacement(piecesRestantes[indicePieceSelectionnee]);

                    // En cas d'erreur de réflection, fait appel à l'aléatoire pour assurer une entrée valide.
                    if (choix.Length < 2 || EstOccuper(choix[0], choix[1]))
                        choix = ReflechirPlacement(piecesRestantes[indicePieceSelectionnee], true);
                }

                // Place la pièce et la retire des disponibles.
                Jouer(indicePieceSelectionnee, choix[0], choix[1]);

                // Tant que le dernier joueur n'a pas gagner et qu'il reste encore des tours possibles.
                if (AGagne() || tour++ >= 16 + 1)
                    break;

                /************************************************
                 * Phase 2: selectionner la pièce à placer après.
                 */

                // Cas d'un joueur humain.
                if (deuxJoueurs || joueur == 1) {
                    Console.WriteLine("\n");
                    Afficher();
                    Console.WriteLine("\nPièces restantes: \n" + DetaillerRestantes());
                    Console.Write("Choisir une pièce pour l'autre joueur: ");

                    // Tant que l'entrée ne corespond pas a une pièce.
                    while ((indicePieceSelectionnee = int.Parse(Console.ReadLine()) - 1) < 0
                           || piecesRestantes.Length - 1 < indicePieceSelectionnee)
                        Console.Write("Veuiller entrer une pièce valable: ");
                }

                // Cas du joueur ordinateur (fonctions Reflechir[..]).
                else {
                    indicePieceSelectionnee = ReflechirPieceSuivante();

                    // En cas d'erreur de réflection, fait appel à l'aléatoire pour assurer une entrée valide.
                    if (indicePieceSelectionnee < 0 || piecesRestantes.Length - 1 < indicePieceSelectionnee)
                        indicePieceSelectionnee = ReflechirPieceSuivante(true);
                }
            } while (true);

            // A la fin de la boucle, `joueur` est le joueur gagnant, sauf si `tour >= 16 + 1`
            if (tour < 16 + 1) {
                Console.WriteLine("Le joueur {0} à gagner!", joueur);
                return joueur;
            }

            Console.WriteLine("Matche nul.");
            return -1;
        }

        static int[] TraduireEntree(String entree) {
            if (entree.Length < 2 // Si la chaine de caractères est trop courtes,
                || entree[0] < 'a' || 'a' + TAILLE < entree[0] // ou l'indicateur de ligne est out of range,
                || entree[1] < '1' || '1' + TAILLE < entree[1]) // ou l'indicateur de colonne est out of range.
                return null;

            return new int[] { entree[0] - 'a', entree[1] - '1' };
        }

        static int[] ReflechirPlacement(String pieceAPlacer, bool estAleatoire = false) {
            if (estAleatoire) {
                int[] r = null;

                do r = new int[] { RNG.Next(TAILLE) + 1,
                                   RNG.Next(TAILLE) + 1 };
                while (EstOccuper(r[0], r[1]));

                return r;
            }

            return ReflechirPlacement(pieceAPlacer, true); // Partie 'intelligente' à faire.
        }

        static int ReflechirPieceSuivante(bool estAleatoire = false) {
            if (estAleatoire)
                return RNG.Next(piecesRestantes.Length);

            return ReflechirPieceSuivante(true); // Partie 'intelligente' à faire.
        }

        static void Jouer(int indice, int posX, int posY) {
            grille[posX, posY] = piecesRestantes[indice]; // Place la pièce.
            EnleverPieces(indice); // Elle n'est plus disponible.
        }

        static bool EstOccuper(int posX, int posY) {
            return grille[posX, posY] != CASE_VIDE;
        }

        static public void EnleverPieces(int indicePieceJouee) //On enlève la pièce déjà jouée
        {
            string[] temp = new string[piecesRestantes.Length - 1]; //On crée un tableau temporaire de longueur égale à celui des pièces restantes -1

            for (int i = 0; i < temp.Length; i++) //pour chaque élément dans le tableau
            {
                if (i < indicePieceJouee) //si on a pas atteint l'indice à enlever
                { temp[i] = piecesRestantes[i]; } //on réécrit le tableau
                if (i >= indicePieceJouee) //si on l'a dépassé 
                { temp[i] = piecesRestantes[i + 1]; } //on le réécrit à partir de l'indice qui suit
            }

            piecesRestantes = temp; //on remplace le tableau précédent par le nouveau
        }

        static public bool AGagne0()
        {
            int[] cptCaracteristiques = new int[4];

            for (int i = 0; i < TAILLE; i++)  //boucle de test des lignes//
            {
                for (int j = 0; j < TAILLE; j++) //Pour chaque colonne dans la ligne i
                {
                    if (grille[i, j][0] == 'n') { cptCaracteristiques[0]++; } //on incrémente le compteur de caractéristiques pour une des deux possibilités
                    else { cptCaracteristiques[0]--; }
                    if (grille[i, j][1] == 'g') { cptCaracteristiques[1]++; }
                    else { cptCaracteristiques[1]--; }
                    if (grille[i, j][2] == 'P') { cptCaracteristiques[2]++; }
                    else { cptCaracteristiques[2]--; }
                    if (grille[i, j][3] == 'r') { cptCaracteristiques[3]++; }
                    else { cptCaracteristiques[3]--; }
                }
            }
            for (int i = 0; i < cptCaracteristiques.Length; i++)
            {
                if (cptCaracteristiques[i] == -4 || cptCaracteristiques[i] == 4) // si une caractéristique à un compteur de 4 ou de 0, c'est qu'il y au moins un point commun sur toute la ligne
                {
                    return true;
                }
            }

            cptCaracteristiques = new int[4];

            for (int i = 0; i < TAILLE; i++)
            {
                for (int j = 0; j < TAILLE; j++) // même chose pour les colonnes
                {
                    if (grille[j, i][0] == 'n') { cptCaracteristiques[0]++; }
                    else { cptCaracteristiques[0]--; }
                    if (grille[j, i][1] == 'g') { cptCaracteristiques[1]++; }
                    else { cptCaracteristiques[1]--; }
                    if (grille[j, i][2] == 'P') { cptCaracteristiques[2]++; }
                    else { cptCaracteristiques[2]--; }
                    if (grille[j, i][3] == 'r') { cptCaracteristiques[3]++; }
                    else { cptCaracteristiques[3]--; }
                }
            }
            for (int i = 0; i < cptCaracteristiques.Length; i++)
            {
                if (cptCaracteristiques[i] == -4 || cptCaracteristiques[i] == 4)
                {
                    return true;
                }
            }

            cptCaracteristiques = new int[4];

            for (int j = 0; j < TAILLE; j++) // et pour les diagonales
            {
                if (grille[j, TAILLE - 1 - j][0] == 'n') { cptCaracteristiques[0]++; }
                else { cptCaracteristiques[0]--; }
                if (grille[j, TAILLE - 1 - j][1] == 'g') { cptCaracteristiques[1]++; }
                else { cptCaracteristiques[1]--; }
                if (grille[j, TAILLE - 1 - j][2] == 'P') { cptCaracteristiques[2]++; }
                else { cptCaracteristiques[2]--; }
                if (grille[j, TAILLE - 1 - j][3] == 'r') { cptCaracteristiques[3]++; }
                else { cptCaracteristiques[3]--; }
            }
            for (int i = 0; i < cptCaracteristiques.Length; i++)
            {
                if (cptCaracteristiques[i] == -4 || cptCaracteristiques[i] == 4)
                {
                    Console.WriteLine("coucou");
                    return true;
                }
            }

            cptCaracteristiques = new int[4];

            for (int j = 0; j < TAILLE; j++) // et pour les diagonales
            {
                if (grille[j, j][0] == 'n') { cptCaracteristiques[0]++; }
                else { cptCaracteristiques[0]--; }
                if (grille[j, j][1] == 'g') { cptCaracteristiques[1]++; }
                else { cptCaracteristiques[1]--; }
                if (grille[j, j][2] == 'P') { cptCaracteristiques[2]++; }
                else { cptCaracteristiques[2]--; }
                if (grille[j, j][3] == 'r') { cptCaracteristiques[3]++; }
                else { cptCaracteristiques[3]--; }
            }
            for (int i = 0; i < cptCaracteristiques.Length; i++)
            {
                if (cptCaracteristiques[i] == -4 || cptCaracteristiques[i] == 4)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool AGagne() {
            // On va compter les caractériqtiques présentes :
            // * noire (n) -> +1, sinon -1
            // * petite (p) -> +1, sinon -1
            // *...
            //
            // si on atteint +4 ou -4, c'est sue le jeux est finit.
            String references = "nptr";

            int[] compteurLigne = new int[4];
            int[] compteurColonne = new int[4];
            int[] compteurDiagonale = new int[4];
            int[] compteurAntidiagonale = new int[4];

            for (int i = 0; i < TAILLE; i++)
                for (int k = 0; k < 4; k++) {
                    for (int j = 0; j < TAILLE && grille[i, j] + grille[j, i] != CASE_VIDE + CASE_VIDE; j++) {
                        compteurLigne[k]+= grille[i, j][k] == references[k] ? 1 : -1;
                        compteurColonne[k] += grille[j, i][k] == references[k] ? 1 : -1;
                    }
                    if (grille[i, i] != CASE_VIDE)
                        compteurDiagonale[k]+= grille[i, i][k] == references[k] ? 1 : -1;
                    if (grille[i, TAILLE-1 - i] != CASE_VIDE)
                        compteurAntidiagonale[k]+= grille[i, TAILLE-1 - i][k] == references[k] ? 1 : -1;

                    Console.WriteLine("ligne {0}: {0}", k, compteurLigne[k]);
                    Console.WriteLine("colonne {0}: {0}", k, compteurColonne[k]);
                    Console.WriteLine("diagonale {0}: {0}", k, compteurDiagonale[k]);
                    Console.WriteLine("antidiagonale {0}: {0}", k, compteurAntidiagonale[k]);

                    // Si on compte 4 fois la même caractéristique sur..
                    if (Math.Abs(compteurLigne[k]) == 4 // .. une ligne,..
                        || Math.Abs(compteurColonne[k]) == 4 // .. une colonne,..
                        || Math.Abs(compteurDiagonale[k]) == 4 // .. la digaonale,..
                        || Math.Abs(compteurAntidiagonale[k]) == 4) // .. ou l'antidiagonale,
                        return true; // c'est que le dernier joueur a gagner.
                }

            // On a trouver aucune condition gagnante.
            return false;
        }

        static public string DetaillerPiece(string pieceChoisie) //Permet d'afficher au joueur les caractéristiques des pièces jouées
        {
            string a = ""; // "La pièce choisie est ";

            if (pieceChoisie[0] == 'n') // Pour chaque caractère
            { a += "noire, "; }
            else { a += "blanche, "; } // on affiche la description correspondante

            if (pieceChoisie[1] == 'p')
            { a += "petite, "; }
            else { a += "grande, "; }

            if (pieceChoisie[2] == 'P')
            { a += "pleine "; }
            else { a += "creuse "; }

            if (pieceChoisie[3] == 'c')
            { a += "et carrée."; }
            else { a += "et ronde."; }

            return a;
        }

        static public string DetaillerRestantes() //Affiche la liste explicitée des pièces restantes et leurs caractéristiques
        {
            string b = " ";

            for (int i = 0; i < piecesRestantes.Length; i++) //Pour chaque pièce restante
            {
                b += (i + 1); //On numérote à i+1
                b += " : ";
                b += DetaillerPiece(piecesRestantes[i]); //Appel à la fonction de définition des pièces
                b += " \n ";
            }

            return b;
        }

    }

}
