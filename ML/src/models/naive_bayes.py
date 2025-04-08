"""
朴素贝叶斯模型模块
"""
from sklearn.naive_bayes import MultinomialNB
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def train_naive_bayes(x_train, y_train, progress_callback=None):
    """
    训练朴素贝叶斯模型
    
    Args:
        x_train: 训练特征
        y_train: 训练标签
        progress_callback: 进度回调函数
        
    Returns:
        object: 训练好的模型
    """
    if progress_callback:
        progress_callback("训练朴素贝叶斯模型")
    
    # 获取模型参数
    config = CONFIG['models']['naive_bayes']
    alpha = config.get('alpha', 1.0)
    fit_prior = config.get('fit_prior', True)
    
    logger.info(f"训练朴素贝叶斯模型: alpha={alpha}, fit_prior={fit_prior}")
    
    # 初始化模型
    model = MultinomialNB(alpha=alpha, fit_prior=fit_prior)
    
    # 训练模型
    model.fit(x_train, y_train)
    
    if progress_callback:
        progress_callback(20)
    
    logger.info("朴素贝叶斯模型训练完成")
    return model 