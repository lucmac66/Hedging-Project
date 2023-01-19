#include "MonteCarlo.hpp"
#include "pnl/pnl_random.h"
#include <iostream>
#include <string>
#include <fstream>
#include "pnl/pnl_vector.h"

MonteCarlo::MonteCarlo(BlackScholesModel *mod, Option *opt, int nbrTirages)
{
    this->mod_ = mod;
    this->opt_ = opt;
    this->nbrTirages_ = nbrTirages;
}

MonteCarlo::~MonteCarlo() {
    opt_->~Option();
    mod_->~BlackScholesModel();
}


void MonteCarlo::priceAndDeltas(const PnlMat *past, double currentDate, bool isMonitoringDate, PnlVect *prices, PnlVect *deltas, PnlVect *deltasStdDev, double epsilon) {
    double price = 0;
    double priceStdDev = 0;
    PnlMat *pathEpsilonP = pnl_mat_create_from_zero(opt_->size_, opt_->size_);
    PnlMat *pathEpsilonN = pnl_mat_create_from_zero(opt_->size_, opt_->size_);
    PnlMat *path = pnl_mat_create_from_zero(opt_->size_, opt_->size_);
    double payoff = 0;

    //random Initialisation
    PnlRng *rng = pnl_rng_create(PNL_RNG_MERSENNE);
    pnl_rng_sseed(rng, (unsigned long)time(NULL));
    // Calcul Prix
    for (int j = 0; j < nbrTirages_; j++) {
        mod_->asset(path, rng, isMonitoringDate, currentDate, past, opt_->dates_);
        payoff = opt_->payoff(path, mod_->r_);
        price += payoff;
        priceStdDev += payoff * payoff;
        for (int i = 0; i < opt_->size_; i++) {
            double deltapayoff = 0;

            pathEpsilonP = pnl_mat_copy(path);
            pathEpsilonN = pnl_mat_copy(path);

            mod_->shiftAsset(pathEpsilonP, past, isMonitoringDate, epsilon, i);
            mod_->shiftAsset(pathEpsilonN, past, isMonitoringDate, -1 * epsilon, i);

            deltapayoff = opt_->payoff(pathEpsilonP, mod_->r_) - opt_->payoff(pathEpsilonN, mod_->r_);
            pnl_vect_set(deltas, i, pnl_vect_get(deltas, i) + deltapayoff);
            pnl_vect_set(deltasStdDev, i, pnl_vect_get(deltasStdDev, i) + deltapayoff);
        }
    }
    for (int a = 0; a < opt_->size_; a++) {
        pnl_vect_set(deltas, a, pnl_vect_get(deltas, a) / (2*epsilon*nbrTirages_));
        pnl_vect_set(deltasStdDev, a, sqrt(abs(pnl_vect_get(deltasStdDev, a)/ (2*epsilon*nbrTirages_) - pnl_vect_get(deltas, a)*pnl_vect_get(deltas, a))));
    }

    price /= nbrTirages_;
    priceStdDev = abs(priceStdDev/nbrTirages_ - price * price);
    priceStdDev = sqrt(priceStdDev);
    pnl_vect_set(prices, 0, price);
    pnl_vect_set(prices, 1, priceStdDev);
    pnl_mat_free(&path);
    pnl_mat_free(&pathEpsilonP);
    pnl_mat_free(&pathEpsilonN);
    pnl_rng_free(&rng);
}