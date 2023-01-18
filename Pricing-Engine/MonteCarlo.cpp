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

MonteCarlo::~MonteCarlo() {}


void MonteCarlo::priceAndDeltas(const PnlMat *past, double epsilon, double currentDate, bool isMonitoringDate, PnlVect* prices, PnlVect *deltas, PnlVect *deltasStdDev)
{
    double price = 0;
    double priceStdDev = 0;
    PnlMat *path = pnl_mat_create_from_zero(opt_->size_, opt_->size_);
    double value;
    double stddevValue;
    PnlMat *pathEpsilon = pnl_mat_create_from_zero(opt_->size_, opt_->size_);
    double payoff = 0;
    double payoffEpsilon = 0;
    pathEpsilon = pnl_mat_create_from_zero((opt_->size_), (opt_->dates_)->size);
    PnlRng *rng = pnl_rng_create(PNL_RNG_MERSENNE);
    pnl_rng_sseed(rng, time(NULL));

    // Calcul des deltas et Prix
    for (int i = 0; i < opt_->size_; i++) {
        value = 0.;
        stddevValue = 0.;
        for (int j = 0; j < nbrTirages_; j++) {
            mod_->asset(path, rng, isMonitoringDate, currentDate, past, opt_->dates_, epsilon, i);
            mod_->asset(pathEpsilon, rng, isMonitoringDate, currentDate, past, opt_->dates_, 0, i);
            payoff = opt_->payoff(path, mod_->r_);
            payoffEpsilon = opt_->payoff(pathEpsilon, mod_->r_);
            value += (payoffEpsilon - payoff);

            stddevValue += (payoffEpsilon - payoff) * (payoffEpsilon - payoff);
            price += payoff;
            priceStdDev += payoff * payoff;
        }
        value /= (epsilon * nbrTirages_);
        stddevValue = sqrt(stddevValue / (epsilon * nbrTirages_) - value * value);
        pnl_vect_set(deltas, i, value);
        pnl_vect_set(deltasStdDev, i, stddevValue);
    }
    
    price /= ((opt_->size_)*nbrTirages_);
    priceStdDev /= ((opt_->size_) * nbrTirages_);
    priceStdDev = sqrt(priceStdDev - price * price);
    pnl_vect_set(prices, 0, price);
    pnl_vect_set(prices, 1, priceStdDev);
}