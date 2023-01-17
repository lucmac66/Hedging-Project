#include "MonteCarlo.hpp"
#include "pnl/pnl_random.h"

MonteCarlo::MonteCarlo(BlackScholesModel *mod, Option *opt, int nbrTirages)
{
    this->mod_ = mod;
    this->opt_ = opt;
    this->nbrTirages_ = nbrTirages;
}

MonteCarlo::~MonteCarlo() {}


void MonteCarlo::priceAndDeltas(const PnlMat *past, double epsilon, double currentDate, bool isMonitoringDate, double price, double priceStdDev, PnlVect *deltas, PnlVect *deltasStdDev)
{
    PnlMat *path = pnl_mat_create_from_zero(opt_->size_, (opt_->dates_)->size);
    double delta;
    double stddevDelta;
    PnlMat *pathEpsilon;
    PnlMat *pastEpsilon; 
    double payoff = 0;
    double payoffEpsilon = 0;
    PnlRng *rng = pnl_rng_create(PNL_RNG_MERSENNE);
    // Calcul des deltas et Prix
    for (int i = 0; i < opt_->size_; i++) {
        delta = 0;
        stddevDelta = 0;
        pathEpsilon = pnl_mat_create_from_zero((opt_->size_), (opt_->dates_)->size);
        pastEpsilon = pnl_mat_copy(past);
        mod_->shiftAsset(pastEpsilon, i, epsilon);

        for (int j = 0; j < this->nbrTirages_; j++) {
            mod_->asset(path, rng, isMonitoringDate, currentDate, past, opt_->dates_);
            mod_->asset(pathEpsilon, rng, isMonitoringDate, currentDate, pastEpsilon, opt_->dates_);
            payoff = opt_->payoff(path, mod_->r_);
            payoffEpsilon = opt_->payoff(pathEpsilon, mod_->r_);

            delta += (payoffEpsilon - payoff);
            stddevDelta += (payoffEpsilon - payoff) * (payoffEpsilon - payoff);
            price += payoff;
            priceStdDev += payoff * payoff;
        }
        delta /= (epsilon * nbrTirages_);
        stddevDelta /= (epsilon * nbrTirages_);
        stddevDelta = sqrt(stddevDelta - delta * delta);
        pnl_vect_set(deltas, i, delta);
        pnl_vect_set(deltasStdDev, i, stddevDelta);
    }

    price /= ((opt_->size_)*nbrTirages_);
    priceStdDev /= ((opt_->size_) * nbrTirages_);
    priceStdDev = sqrt(priceStdDev - price * price);
}