#pragma once

#include "Option.hpp"
#include "BlackScholesModel.hpp"
#include "pnl/pnl_random.h"

class MonteCarlo
{
public:
    BlackScholesModel *mod_; /*! pointeur vers le modèle */
    Option *opt_; /*! pointeur sur l'option */
    int nbrTirages_;

    MonteCarlo(BlackScholesModel *mod, Option *opt, int nbrTirages);

    ~MonteCarlo();
    /**
     * Calcule le prix de l'option à la date 0
     *
     * @return valeur de l'estimateur Monte Carlo
     */
    void priceAndDeltas(const PnlMat *past, double epsilon, double currentDate, bool isMonitoringDate, PnlVect *prices, PnlVect *deltas, PnlVect *deltasStdDev);
};


