export const parsePrice = (price: string | number): number => {
  if (typeof price === 'number') {
    return price;
  }
  if (typeof price === 'string') {
    // This will remove 'RD$', commas, and keep the decimal point.
    const numericString = price.replace(/[^0-9.-]+/g, '');
    const number = parseFloat(numericString);
    return isNaN(number) ? 0 : number;
  }
  // Return 0 if the price is not a string or number
  return 0;
};
