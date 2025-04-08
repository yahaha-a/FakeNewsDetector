"""
流水线构建模块，用于创建和配置机器学习流水线
"""
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer, CountVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.ensemble import RandomForestClassifier
from sklearn.svm import SVC
from sklearn.linear_model import LogisticRegression
from sklearn.model_selection import GridSearchCV
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def create_pipeline(model_type='naive_bayes', vectorizer_type='tfidf', stopwords=None):
    """
    创建机器学习流水线
    
    Args:
        model_type: 模型类型
        vectorizer_type: 向量化器类型
        stopwords: 停用词列表
        
    Returns:
        object: 流水线对象
    """
    # 获取配置
    features_config = CONFIG['features']
    models_config = CONFIG['models']
    
    # 创建向量化器
    if vectorizer_type == 'tfidf':
        config = features_config['tfidf']
        vectorizer = TfidfVectorizer(
            min_df=config.get('min_df', 1),
            max_features=config.get('max_features', None),
            ngram_range=tuple(config.get('ngram_range', [1, 1]))
        )
    else:  # 'count'
        config = features_config['countvec']
        vectorizer = CountVectorizer(
            min_df=config.get('min_df', 1),
            max_features=config.get('max_features', None),
            ngram_range=tuple(config.get('ngram_range', [1, 1])),
            stop_words=stopwords if config.get('use_stopwords', True) else None
        )
    
    # 创建模型
    if model_type == 'naive_bayes':
        config = models_config['naive_bayes']
        model = MultinomialNB(
            alpha=config.get('alpha', 1.0),
            fit_prior=config.get('fit_prior', True)
        )
    elif model_type == 'random_forest':
        config = models_config['random_forest']
        model = RandomForestClassifier(
            n_estimators=config.get('n_estimators', [200])[0],  # 使用第一个值作为默认值
            max_depth=config.get('max_depth', [10])[0],
            random_state=config.get('random_state', 42)
        )
    elif model_type == 'svm':
        config = models_config['svm']
        model = SVC(
            kernel=config.get('kernel', 'rbf'),
            C=config.get('C', 1.0),
            gamma=config.get('gamma', 'scale'),
            random_state=config.get('random_state', 42),
            probability=True  # 启用概率估计以计算AUC
        )
    else:  # 'logistic'
        config = models_config['logistic_regression']
        model = LogisticRegression(
            C=config.get('C', 1.0),
            max_iter=config.get('max_iter', 100),
            random_state=config.get('random_state', 42)
        )
    
    # 创建流水线
    pipeline = Pipeline([
        ('vectorizer', vectorizer),
        ('classifier', model)
    ])
    
    logger.info(f"创建流水线: {vectorizer_type}向量化器 + {model_type}模型")
    return pipeline


def create_grid_search_pipeline(model_type='naive_bayes', vectorizer_type='tfidf', stopwords=None):
    """
    创建带有网格搜索的流水线
    
    Args:
        model_type: 模型类型
        vectorizer_type: 向量化器类型
        stopwords: 停用词列表
        
    Returns:
        object: 网格搜索流水线对象
    """
    # 获取配置
    features_config = CONFIG['features']
    models_config = CONFIG['models']
    evaluation_config = CONFIG['evaluation']
    
    # 创建基础流水线
    pipeline = create_pipeline(model_type, vectorizer_type, stopwords)
    
    # 定义参数网格
    param_grid = {}
    
    # 添加向量化器参数
    if vectorizer_type == 'tfidf':
        param_grid.update({
            'vectorizer__min_df': [1, 2, 3],
            'vectorizer__max_features': [5000, 10000, None],
            'vectorizer__ngram_range': [(1, 1), (1, 2)]
        })
    else:  # 'count'
        param_grid.update({
            'vectorizer__min_df': [1, 2, 3],
            'vectorizer__max_features': [5000, 10000, None],
            'vectorizer__ngram_range': [(1, 1), (1, 2)]
        })
    
    # 添加模型参数
    if model_type == 'naive_bayes':
        param_grid.update({
            'classifier__alpha': [0.1, 0.5, 1.0, 2.0],
            'classifier__fit_prior': [True, False]
        })
    elif model_type == 'random_forest':
        param_grid.update({
            'classifier__n_estimators': models_config['random_forest'].get('n_estimators', [100, 200, 300]),
            'classifier__max_depth': models_config['random_forest'].get('max_depth', [5, 10, 15])
        })
    elif model_type == 'svm':
        param_grid.update({
            'classifier__C': [0.1, 1.0, 10.0],
            'classifier__gamma': ['scale', 'auto', 0.01, 0.1],
            'classifier__kernel': ['rbf', 'linear']
        })
    else:  # 'logistic'
        param_grid.update({
            'classifier__C': [0.1, 1.0, 10.0],
            'classifier__max_iter': [100, 200, 300]
        })
    
    # 创建网格搜索
    grid_search = GridSearchCV(
        pipeline,
        param_grid=param_grid,
        cv=evaluation_config.get('cv_folds', 5),
        scoring='f1',
        n_jobs=-1,  # 使用所有可用CPU核心
        verbose=1
    )
    
    logger.info(f"创建网格搜索流水线: {len(param_grid)}个参数")
    return grid_search 