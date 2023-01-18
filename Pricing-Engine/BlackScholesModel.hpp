#pragma once

#include "pnl/pnl_random.h"
#include "pnl/pnl_vector.h"
#include "pnl/pnl_matrix.h"

/// \brief Modèle de Black Scholes
class BlackScholesModel
{
public:
    int size_; /// nombre d'actifs du modèle
    double r_; /// taux d'intérêt
    PnlMat *volatility_; /// vecteur de volatilités
    PnlVect *divid_; /// vecteur des dividendes

    BlackScholesModel(double interestRate, PnlMat* volatility, PnlVect* divids);
    /**
     * Génère une trajectoire du modèle et la stocke dans path
     *
     * @param[out] path contient une trajectoire du modèle.
     * C'est une matrice de taille (nbTimeSteps+1) x d
     * @param[in] T  maturité
     * @param[in] nbTimeSteps nombre de dates de constatation
     */
    void asset(PnlMat *path, PnlRng *rng, double isMonitoring, double currentDate, const PnlMat *past, PnlVect *dates, double epsilon, int j);

    void shiftAsset(PnlMat *past, int j, double epsilon);

};
