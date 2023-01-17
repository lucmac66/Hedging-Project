#include "BlackScholesModel.hpp"


BlackScholesModel::BlackScholesModel(double interestRate, PnlMat *volatility, PnlVect *divids) {
    this->size_ = volatility->n;
    this->r_ = interestRate;
    this->volatility_ = volatility;
    this->divid_ = divids;
}

void BlackScholesModel::asset(PnlMat *path, PnlRng *rng, double isMonitoring, double currentDate, const PnlMat *past, PnlVect* dates){

    // Initialisation de Variables
    PnlVect *L = pnl_vect_create_from_zero(size_);
    PnlVect *brownian = pnl_vect_create_from_zero(size_);

    // matrice G de la loi normale
    PnlMat *G = pnl_mat_new();
    pnl_mat_rng_normal(G, size_, (path->m) - (past->m) + 1, rng);

    // Completion Initiale de la matrice en sortie
    pnl_mat_set_subblock(path, past, 0, 0);

    // Gestion du MonitoringDate
    double timeStep = 0;
    int start = 0;
    int pas = 0;

    if (isMonitoring) {
        start = (past->m);
        timeStep = pnl_vect_get(dates, start) - pnl_vect_get(dates, start - 1);
        pas = -1;
    } else {
        start = (past->m) - 1;
        timeStep = pnl_vect_get(dates, start) - currentDate;
        pas = 0;
    }

    // Simulation du reste de la matrice
    for (int underlyingAsset = 0; underlyingAsset < size_; underlyingAsset++) 
    {
        pnl_mat_get_row(L, volatility_, underlyingAsset);
        double sigma2 = pnl_vect_scalar_prod(L, L);
        double delta = pnl_vect_get(divid_, underlyingAsset);

        for (int k = start; k < size_; k++)
        {
            pnl_mat_get_row(brownian, G, k-start);
            double scalar_product = pnl_vect_scalar_prod(L, brownian);
            double new_price = pnl_mat_get(path, underlyingAsset, k+pas) * exp((r_ - delta - (sigma2 / 2)) * (timeStep) + (sqrt(timeStep) * scalar_product));
            pnl_mat_set(path, underlyingAsset, k, new_price);
            double timeStep = pnl_vect_get(dates, k+1) - pnl_vect_get(dates, k);
        }
    }
}

void BlackScholesModel::shiftAsset(PnlMat *past, int j, double epsilon) {
    // On shift juste la valeur de S_j en t puis on simulera le reste des trajectoires shiftées avec la méthode asset
    double newValue = pnl_mat_get(past, j, past->m) + epsilon;
    pnl_mat_set(past, j, past->m, newValue);
}
