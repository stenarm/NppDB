﻿namespace NppDB.Comm
{
    public enum ParserMessageType
    {
        NONE,
        EQUALS_ALL,
        NOT_EQUALS_ANY,
        DOUBLE_QUOTES,
        SELECT_ALL_IN_SUB_QUERY,
        MULTIPLE_COLUMNS_IN_SUB_QUERY,
        SELECT_ALL_IN_UNION_STATEMENT,
        SELECT_ALL_IN_INSERT_STATEMENT,
        SELECT_ALL_WITH_MULTIPLE_JOINS,
        DISTINCT_KEYWORD_WITH_GROUP_BY_CLAUSE,
        AGGREGATE_FUNCTION_WITHOUT_GROUP_BY_CLAUSE,
        AGGREGATE_FUNCTION_IN_GROUP_BY_CLAUSE,
        AGGREGATE_FUNCTION_IN_WHERE_CLAUSE,
        COUNT_FUNCTION_WITH_OUTER_JOIN,
        INSERT_STATEMENT_WITHOUT_COLUMN_NAMES,
        ORDERING_BY_ORDINAL,
        HAVING_CLAUSE_WITHOUT_AGGREGATE_FUNCTION,
        DUPLICATE_SELECTED_COLUMN_IN_SELECT_CLAUSE,
        AND_OR_MISSING_PARENTHESES_IN_WHERE_CLAUSE,
        AND_OR_MISSING_PARENTHESES_IN_HAVING_CLAUSE,
        MISSING_COLUMN_ALIAS_IN_SELECT_CLAUSE,
        USE_COUNT_FUNCTION,
        USE_AVG_FUNCTION,
        ORDER_BY_CLAUSE_IN_SUB_QUERY_WITHOUT_LIMIT,
        MISSING_WILDCARDS_IN_LIKE_EXPRESSION,
        COLUMN_LIKE_COLUMN,
        EQUALITY_WITH_NULL,
        EQUALITY_WITH_TEXT_PATTERN,
        NOT_LOGICAL_OPERAND,
        WARNING_FORMAT,
        TOP_KEYWORD_WITHOUT_ORDER_BY_CLAUSE,
        MISSING_COLUMN_IN_GROUP_BY_CLAUSE,
        MULTIPLE_WHERE_USED,
        MISSING_EXPRESSION_IN_JOIN_CLAUSE,
        UNNECESSARY_SEMICOLON,
        MISSING_EXPRESSION_IN_WHERE_CLAUSE,
        TOP_KEYWORD_MIGHT_RETURN_MULTIPLE_ROWS,
        POSSIBLE_NON_INTEGER_VALUE_WITH_TOP,
        TOP_LIMIT_CONSTRAINT,
        TOP_LIMIT_PERCENT_CONSTRAINT,
        ONE_ROW_IN_RESULT_WITH_TOP,
        MISSING_ALIAS_IN_FROM_SUBQUERY,
        MISSING_EXPRESSION_IN_HAVING_CLAUSE,
        SUBQUERY_COLUMN_COUNT_MISMATCH,
        COMPARING_WITH_NULL,
        NOT_EQUALITY_WITH_NULL,
        MULTIPLE_HAVING_USED,
        FETCH_CLAUSE_MIGHT_RETURN_MULTIPLE_ROWS,
        LIMIT_CONSTRAINT,
        ONE_ROW_IN_RESULT_WITH_LIMIT,
        FETCH_LIMIT_OFFSET_CLAUSE_WITHOUT_ORDER_BY_CLAUSE,
        PARSING_ERROR,
    }
}