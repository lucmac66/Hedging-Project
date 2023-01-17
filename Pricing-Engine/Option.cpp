#include "pnl/pnl_vector.h"
#include "pnl/pnl_matrix.h"
#include "Option.hpp"
#include "math.h"
/// \brief Classe Option

Option::Option(PnlVect *strikes, PnlVect *paymentDates, int size) {
    this->dates_ = paymentDates;
    this->strike_ = strikes;
    this->size_ = size;
}

Option::~Option() {}

/**
 * Calcule la valeur du payoff sur la trajectoire
 *
 * @param[in] path est une matrice de taille (N+1) x d
 * contenant une trajectoire du modèle telle que créée
 * par la fonction asset.
 * @return phi(trajectoire)
 */
double Option::payoff(const PnlMat *path, double rate)
{
    // On set-up des variables pour calculer le payoff
    double payoff = 0;
    double asset = 0;
    double strike = 0;
    double T = pnl_vect_get(dates_, size_-1);
    // On regarde la valeur du payoff
    for (int k = 0; k < size_; k++) {
        asset = pnl_mat_get(path, k, k);
        strike = pnl_vect_get(strike_, k);
        if (asset > strike) {
            double t = pnl_vect_get(dates_, k);
            payoff = exp(-rate * (T - t)) * (asset - strike);
            return payoff;
        }
        
    }
    return payoff;
}