import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons, kinds } from 'Helpers/Props';
import styles from './HistoryEventTypeCell.css';

function getIconName(eventType) {
  switch (eventType) {
    case 'indexerQuery':
      return icons.SEARCH;
    case 'releaseGrabbed':
      return icons.DOWNLOAD;
    default:
      return icons.UNKNOWN;
  }
}

function getIconKind(data) {
  switch (data.successful) {
    case 'False':
      return kinds.DANGER;
    default:
      return kinds.DEFAULT;
  }
}

function getTooltip(eventType, data, indexer) {
  switch (eventType) {
    case 'indexerQuery':
      return `Query "${data.query}" sent to ${indexer.name}`;
    case 'releaseGrabbed':
      return `Release grabbed from ${indexer.name}`;
    default:
      return 'Unknown event';
  }
}

function HistoryEventTypeCell({ eventType, data, indexer }) {
  const iconName = getIconName(eventType);
  const iconKind = getIconKind(data);
  const tooltip = getTooltip(eventType, data, indexer);

  return (
    <TableRowCell
      className={styles.cell}
      title={tooltip}
    >
      <Icon
        name={iconName}
        kind={iconKind}
      />
    </TableRowCell>
  );
}

HistoryEventTypeCell.propTypes = {
  eventType: PropTypes.string.isRequired,
  data: PropTypes.object,
  indexer: PropTypes.object
};

HistoryEventTypeCell.defaultProps = {
  data: {}
};

export default HistoryEventTypeCell;
