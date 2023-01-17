#pragma once

#include "pnl/pnl_vector.h"
#include "pnl/pnl_matrix.h"

/// \brief Classe Option abstraite
class Option
{
public:
    PnlVect* dates_; /// nombre de dates d'exercice
    PnlVect* strike_; /// dimension du modèle, redondant avec BlackScholesModel::size_
    int size_;

    Option(PnlVect *strikes, PnlVect *paymentDates, int size);

    ~Option();
    /**
     * Calcule la valeur du payoff sur la trajectoire
     *
     * @param[in] path est une matrice de taille (dates_+1) x size_
     * contenant une trajectoire du modèle telle que créée
     * par la fonction asset.
     * @return phi(trajectoire)
     */

    double payoff(const PnlMat *path, double rate);
};


