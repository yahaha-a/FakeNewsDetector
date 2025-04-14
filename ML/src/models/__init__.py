"""
模型包

提供各种机器学习模型的训练函数，支持朴素贝叶斯、随机森林、支持向量机和逻辑回归等算法。
"""
from src.models.naive_bayes import train_naive_bayes
from src.models.random_forest import train_random_forest
from src.models.svm import train_svm
from src.models.logistic_regression import train_logistic_regression

__all__ = [
    'train_naive_bayes',
    'train_random_forest',
    'train_svm',
    'train_logistic_regression'
] 