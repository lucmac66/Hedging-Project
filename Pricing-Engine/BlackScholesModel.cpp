#include "BlackScholesModel.hpp"
#include <iostream>
#include <string>
#include <fstream>

BlackScholesModel::BlackScholesModel(double interestRate, PnlMat *volatility, PnlVect *divids) {
    this->size_ = volatility->n;
    this->r_ = interestRate;
    this->volatility_ = volatility;
    this->divid_ = divids;
}

void BlackScholesModel::asset(PnlMat *path, PnlRng* rng, double isMonitoring, double currentDate, const PnlMat *past, PnlVect* dates){
    // Initialisation de Variables
    PnlVect *L = pnl_vect_create_from_zero(size_);
    PnlVect *brownian = pnl_vect_create_from_zero(size_);

    // matrice G de la loi normale
    PnlMat *G = pnl_mat_create_from_zero(size_, size_);
    pnl_mat_rng_normal(G, size_, size_, rng);


    PnlVect *lastValues = pnl_vect_create_from_zero(size_);
    for (int i = 0; i < size_; i++) {
        pnl_vect_set(lastValues, i, pnl_mat_get(past, past->m - 1, i));
    }
    // Completion Initiale de la matrice en sortie

    PnlVect *copy = pnl_vect_create_from_zero(size_);
    if (!isMonitoring) {
        for (int i = 0; i < (past->m - 1); i++) {
            pnl_mat_get_row(copy, past, i);
            pnl_mat_set_row(path, copy , i);
        }
    } else {
        for (int i = 0; i < (past->m); i++) {
            pnl_mat_get_row(copy, past, i);
            pnl_mat_set_row(path, copy, i);
        }
    
    }
    // Gestion du MonitoringDate
    int start = 0;
    double timeStep = 0;
    double newPrice = 0;

    if (isMonitoring) {
        start = (past->m);   
    } 
    else {
        start = (past->m) - 1;
    }

    // Simulation du reste de la matrice
    for (int k = start; k < path->m; k++) {
        pnl_mat_get_col(brownian, G, k - start);
        if (k == start) {
            timeStep = pnl_vect_get(dates, k) - currentDate;
            for (int i = 0; i < path->m; i++) {
                pnl_mat_get_row(L, volatility_, i);
                double sigma2 = pnl_vect_scalar_prod(L, L);
                double delta = pnl_vect_get(divid_, i);
                double scalar_product = pnl_vect_scalar_prod(L, brownian);
                newPrice = pnl_vect_get(lastValues, i) * exp((r_ - delta - sigma2 / 2) * timeStep + sqrt(timeStep) * scalar_product);
                pnl_mat_set(path, k, i, newPrice);
            }

        } else {
            for (int i = 0; i < path->m; i++) {
                pnl_mat_get_row(L, volatility_, i);
                double sigma2 = pnl_vect_scalar_prod(L, L);
                double delta = pnl_vect_get(divid_, i);
                double scalar_product = pnl_vect_scalar_prod(L, brownian);
                newPrice = pnl_mat_get(path, k - 1, i) * exp((r_ - delta - sigma2 / 2) * timeStep + sqrt(timeStep) * scalar_product);
                pnl_mat_set(path, k, i, newPrice);
            }
        }
        if (k < path->m - 1) {
            timeStep = pnl_vect_get(dates, k + 1) - pnl_vect_get(dates, k);
        }
    }
    
    pnl_vect_free(&L);
    pnl_vect_free(&brownian);
    pnl_vect_free(&copy);
    pnl_vect_free(&lastValues);
}

void BlackScholesModel::shiftAsset(PnlMat *path, const PnlMat *past, bool isMonitoring, double epsilon, int j) {
    int start = 0;
    int pas = 0;
    PnlVect *col = pnl_vect_create_from_zero(size_);
    pnl_mat_get_col(col, path, j);

    if (isMonitoring) {
         start = (past->m);
    } else {
         start = (past->m) - 1;
    }

    for (int k = start; k < size_; k++) {
         pnl_vect_set(col, k, pnl_vect_get(col,k) + epsilon);
    }

    pnl_mat_set_col(path, col, j);
    pnl_vect_free(&col);
}


BlackScholesModel::~BlackScholesModel() {
    pnl_vect_free(&divid_);
    pnl_mat_free(&volatility_);
}